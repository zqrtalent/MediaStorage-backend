using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MediaStorage.IO;
using MediaStorage.Common.Dtos.Encoder;
using MediaStorage.Common.Dtos.Audio;
using MediaStorage.Encoder.Mp3.Tags;
using Newtonsoft.Json;
using MediaStorage.Common.Dtos.Encoder.Mp3;

namespace MediaStorage.Encoder.Mp3
{
    public class Mp3Encoder : IMediaEncoder
    {
        public Mp3Encoder()
        {
        }

        public Mp3Encoder(IStorageFile fileStream, bool readTags = false)
        {
            _readTags = readTags;
            if(fileStream != null)
                Init(fileStream, readTags);
        }

        public bool Init(IStorageFile stream, bool readTags, string stateJson = null)
        {
            if(stream == null)
                throw new ArgumentNullException($"Null argument {nameof(stream)} !");
                
            _readTags = readTags;
            _fileStream = stream;
            bsbuf = bsspace_1;

            _fileStream.UseReadBuffer(2048);

            bool stateIsRestored = false;
            if( !string.IsNullOrEmpty(stateJson) )
            {
                var state = JsonConvert.DeserializeObject<Mp3EncoderState>(stateJson);
                if(state != null)
                {
                    Args = state.Args;
                    bsnum = state.bsnum;
                    firsthead = state.firsthead;
                    fr = state.frame;
                    framesize = state.framesize;
                    fsize = state.fsize;
                    fsizeold = state.fsizeold;
                    oldhead = state.oldhead;
                    ssize = state.ssize;
                    syncword = state.syncword;
                    
                    _frameOffsets = (state.Args.FramesFileOffsets?.ToList() ?? null);
                    // Restore full offset instead of relative offset value.
                    if(_frameOffsets != null)
                    {
                        for(int i=1; i<_frameOffsets.Count; i++)
                            _frameOffsets[i] += _frameOffsets[i-1];
                    }

                    // Seek to the beggining of the current frame.
                    _fileStream.Seek(state.FileOffset, SeekOrigin.Begin);

                    // Read current frame data.                    
                    //ReadFrame();

                    stateIsRestored = true;
                }
            }

            if(!stateIsRestored)
                ReadFrame();
            return true;
        }

        public bool SaveStateIntoJson(bool analyzeAllFrames, out string stateJson)
        {
            long filePos = _fileStream.Seek(0, SeekOrigin.Current) - (framesize + 4/*header size*/);

            if(analyzeAllFrames)
                this.AnalyzeAllFrames();

            var state = new Mp3EncoderState
            {
                Args = Args,
                bsnum = bsnum,
                firsthead = firsthead,
                frame = fr,
                framesize = framesize,
                fsize = fsize,
                fsizeold = fsizeold,
                oldhead = oldhead,
                ssize = ssize,
                syncword = syncword,
                FileOffset = filePos
            };

            stateJson = JsonConvert.SerializeObject(state);
            return true;
        }

        public IMediaMetadata GetMetadata()
        {
            return new Mp3Metadata
            {
                Album = Album,
                Artist = Artist,
                Bitrate = Bitrate,
                Composer = Composer,
                DurationSec = TotalSec,
                Genre = Genre,
                IsVBR = Args.IsVBR,
                Song = Song,
                Track = Track,
                Year = Year
            };
        }

        public IEnumerable<AttachedPicture> AttachedPictures => _attachedPictures;

        #region Variables
        private IStorageFile _fileStream;
        private bool _readTags = false;
        private IList<long> _frameOffsets; // Frame file offset.
        private Frame fr = new Frame();
        private MPArgs Args = new MPArgs();

        internal void SetFrameFileOffsets(IEnumerable<long> fileOffsets) { this.Args.FramesFileOffsets = fileOffsets?.ToArray() ?? null; }

        private bool framebuffIsEmpty = true;
        private byte[] bsspace_1 = new byte[Constants.MAXFRAMESIZE + 512];
        private byte[] bsspace_2 = new byte[Constants.MAXFRAMESIZE + 512];
        private byte[] bsbuf;
        private byte[] bsbufold;

        public long CurrentFrameFileOffset { get; private set; } // Current frame file offset starting with frame header bytes.
        private int framesize { get; set; }
        private int fsize { get; set; }
        private int fsizeold { get; set; }
        private int ssize { get; set; } // Side info size of the frame.
        private int bsnum { get; set; }
        private ulong oldhead { get; set; }
        private ulong firsthead { get; set; }
        private ulong syncword { get; set; }
        #endregion

        #region Mp3 info properties
        public long SampleRate => Constants.freqs[fr.sampling_frequency];
        public long Framesize => framesize;
        public int SamplesPerFrame => Constants.samplesperframes[fr.lsf, fr.lay - 1];    
        public long MaxFrames => (Args.IsVBR ? Args.NumFrames : (_fileStream.Length - Args.FirstFrameOffset) / (Framesize + 5 - fr.padding));
        // Returns the milli seconds one frame has
        public double Frame2ms => Constants.ms_per_frame_array[1, fr.sampling_frequency];
        public double TotalMs => (MaxFrames * Frame2ms);
        public int TotalSec => (int)(TotalMs / 1000.0);
        public uint CurrentMs => (uint)(Args.CurrentPos * Frame2ms);
        public int Bitrate => Constants.BitRateIndex[fr.lsf, fr.lay - 1, fr.bitrate_index] * 1000;
        #endregion

        #region ID3v2 tags.
        private IEnumerable<AttachedPicture> _attachedPictures;
        public bool HasAttachedPicture => _attachedPictures?.Any() ?? false;
        public string Album { get; protected set; }
        public string Artist { get; protected set; }
        public string Song { get; protected set; }
        public string Composer { get; protected set; }
        public string Genre { get; protected set; }
        public string Track { get; protected set; }
        public string Year { get; protected set; }
        #endregion

        public ulong GetMediaPacketsCount()
        {
            return (ulong)MaxFrames;
        }

        public AudioPackets ReadPackets(int offset, int numPackets)
        {
            if (!Seek_StreamByPos((long)offset, false))
                return null;
            return ReadFrames(numPackets);
        }

        public AudioPackets ReadPacketsByTime(ulong milliSecond, int numPackets)
        {
            if (!Seek_Stream(milliSecond, false))
                return null;
            return ReadFrames(numPackets);
        }

        public AudioPackets ReadFrames(int packetsCt)
        {
            bool isEof = false;
            int packets = 0, frameSizeWithHeader = framesize + 4;
            var mediaFrames = new List<AudioPacket>();
            byte[] frameData = null;

            // Calculate read buffer length in order to read all the freames in only one IO-READ call.
            if(packetsCt > 1)
            {
                if((_frameOffsets?.Count ?? 0) > 0)
                {
                    bool lastPacket = ((int)Args.CurrentPos) + packetsCt >= _frameOffsets.Count;
                    long lastPacketOffset = lastPacket ? _frameOffsets.Last() + Constants.MAXFRAMESIZE : 
                        _frameOffsets[(int)(Args.CurrentPos + (ulong)packetsCt)];
                    int readBufferSize = (int)(lastPacketOffset - _frameOffsets[(int)Args.CurrentPos]);
                    _fileStream.UseReadBuffer(readBufferSize);
                }
                else
                    _fileStream.UseReadBuffer(frameSizeWithHeader * packetsCt);
            }

            // Read frame in buffer first.
            if(framebuffIsEmpty)
            {
                ReadFrame();
                Args.CurrentPos ++;
            }
            
            // Read current frame packet data.
            frameData = new byte[frameSizeWithHeader];
            Array.Copy(bsbuf, 512, frameData, 0, frameSizeWithHeader);

            mediaFrames.Add(new AudioPacket()
            {
                Data = frameData,
                SamplesPerFrame = Args.IsVBR ? SamplesPerFrame : 0
            });
            packets ++;
            
            while (packets < packetsCt)
            {
                if(((long)Args.CurrentPos) >= MaxFrames )
                    break;

            read_again:
                int res = ReadFrame(); // Read next frame
                if (res == 0 && res != -1)
                    goto read_again;

                if (res == -2)
                {
                    isEof = true;
                    break;
                }

                frameSizeWithHeader = framesize + 4;
                // Read current frame packet data.
                frameData = new byte[frameSizeWithHeader];
                Array.Copy(bsbuf, 512, frameData, 0, frameSizeWithHeader);
                mediaFrames.Add(new AudioPacket()
                {
                    Data = frameData,
                    SamplesPerFrame = Args.IsVBR ? SamplesPerFrame : 0
                });

                packets ++;
                Args.CurrentPos ++;
            }

            return new AudioPackets()
            {
                IsEof = (isEof || Args.CurrentPos >= (ulong)MaxFrames),
                IsVBR = Args.IsVBR,
                NumPackets = packets,
                SamplesPerFrame = Args.IsVBR ? 0 : SamplesPerFrame,
                FramesCt = (ulong)MaxFrames,
                Offset = Args.CurrentPos,
                Packets = mediaFrames
            };
        }

        public bool Seek_Stream(ulong Position, bool readFrame = true)
        {
            return Seek_StreamByPos((long)(Position / Frame2ms), readFrame);
        }

        public bool Seek_StreamByPos(long pos/*packet position*/, bool readFrame = true)
        {
            long Posi = pos;
            long OldPosi, XPosi;

            if (Posi >= 0 && Posi <= MaxFrames)
            {
                if(((long)Args.CurrentPos) != pos) // Check current position.
                {
                    OldPosi = _fileStream.Seek(0, SeekOrigin.Current);

                    if(this._frameOffsets != null && this._frameOffsets.Count > pos)
                    {
                        XPosi = this._frameOffsets[(int)pos];
                    }
                    else
                    {
                        if (Args.IsVBR)
                        {
                            int index = (int)((pos * 100.0) / Args.NumFrames);
                            XPosi = (long)(((Args.TOC[index] * 1.0) / 256.0) * Args.FileLength) + Args.FirstFrameOffset;
                        }
                        else
                            XPosi = ((Framesize + 4) * Posi) + Args.FirstFrameOffset;
                    }

                    try
                    {
                        _fileStream.Seek(XPosi, SeekOrigin.Begin);
                    }
                    catch (IOException)
                    {
                        _fileStream.Seek(OldPosi, SeekOrigin.Begin);
                        return false;
                    }

                    Args.CurrentPos = (ulong)Posi;
                }
                
                if(readFrame)
                {
                    int Res;
                read_again:
                    Res = ReadFrame();
                    if (Res != 0)
                        Args.CurrentPos++;
                    if (Res == 0 && Res != -2)
                        goto read_again;
                }
                else
                {
                    framebuffIsEmpty = true; // Read frame of current packet.
                }

                return true;
            }
            
            return false;
        }

        public int ReadFrame()
        {
            byte[] ssave = new byte[34];
            int numread = 0, tryme = 0;
            byte[] hbuf = new byte[8], headbuf = new byte[4];
            ulong newhead = 0;
            bool checkVbr = false;

            if (!head_read(headbuf, ref newhead))
                return -2;

            if (oldhead != newhead || oldhead == 0)
            {
                fr.header_change = 1;
            init_resync:
                if (/*!firsthead && */!head_check(newhead))
                {
                    if ((newhead & Constants.ID3) == Constants.ID3) // ID3
                    {
                        if (!SkipID3v2Tag(hbuf, ref newhead))
                        {
                            //LastError = peInputError;
                            //throw 1;
                        }
                        checkVbr = true;
                    }
                    int i;
                    {
                        for (i = 0; i < Constants.MAX_RESYNC; i++)
                        {
                            Array.Copy(hbuf, 0, hbuf, 1, 3);

                            numread = _fileStream.Read(hbuf, 3, 1);
                            if (numread == 0)
                                return -2;
                            //if (numread != 1)
                            //    throw 1;
                            newhead <<= 8;
                            newhead |= hbuf[3];
                            newhead &= 0xffffffff;
                            if (head_check(newhead)) break;
                        }
                        if (i == Constants.MAX_RESYNC)
                            return 0;
                    }
                }
                if ((newhead & 0xffe00000) != 0xffe00000)
                {
                    if (Args.TryResync)
                    {
                        do
                        {
                            tryme++;
                            Array.Copy(hbuf, 0, hbuf, 1, 7);

                            numread = _fileStream.Read(hbuf, 3, 1);
                            if (numread == 0)
                                return -2;
                            //if (numread != 1)
                            //    throw 1;
                            newhead = ((newhead << 8) | hbuf[3]) & 0xffffffff;
                            if (oldhead == 0) goto init_resync;
                        }
                        while ((newhead & Constants.HDRCMPMASK) != (oldhead & Constants.HDRCMPMASK) && 
                            (newhead & Constants.HDRCMPMASK) != (firsthead & Constants.HDRCMPMASK));
                    }
                    else
                        return 0;
                }

                 if (firsthead == 0)
                     firsthead = newhead;

                if (!decode_header(newhead, checkVbr))
                {
                    //LastError = peInputError;
                    return 0;
                }
                checkVbr = false;
            }
            else
                fr.header_change = 0;

            fsizeold = fsize;
            bsbufold = bsbuf;
            if (bsnum == 1)
            {
                bsbuf = bsspace_2;
                bsnum = 0;
            }
            else
            {
                bsbuf = bsspace_1;
                bsnum = 1;
            }

            fsize = framesize;
            numread = _fileStream.Read(bsbuf, 516, fsize);
            if (numread == 0)
                return -2;

            // Copy header bytes.
            Array.Copy(headbuf, 0, bsbuf, 512, 4);
            if (numread != fsize)
            {
                Array.Clear(bsbuf, numread + 516, fsize - numread);
            }

            CurrentFrameFileOffset = _fileStream.Seek(0, SeekOrigin.Current) - framesize - 4 /*frame header*/;
            if (Args.CurrentPos == 0 && Args.FirstFrameOffset == 0)
            {
                Args.FirstFrameOffset = CurrentFrameFileOffset;
            }

            // Update sample rate value after reading the frame.
            if(Args.SampleRate == 0 || Args.IsVBR)
                Args.SampleRate = Constants.freqs[fr.sampling_frequency];

            this.framebuffIsEmpty = false;
            return 1;
        }

        private bool head_read(byte[] hbuf, ref ulong newhead)
        {
            long numread = _fileStream.Read(hbuf, 0, 4);
            if (numread != 4) return false;
            newhead = ((ulong)hbuf[0] << 24) | ((ulong)hbuf[1] << 16) | ((ulong)hbuf[2] << 8) | (ulong)hbuf[3];
            return true;
        }

        private bool head_check(ulong head)
        {
            if ((head & 0xffe00000) != 0xffe00000)
                return false;
            if ((head & 0xffff0000) == 0xffff0000)
                return false;
            if (((head >> 17) & 3) == 0)
                return false;
            if (((head >> 12) & 0xf) == 0xf)
                return false;
            if (((head >> 10) & 0x3) == 0x3)
                return false;
            return true;
        }

        private bool decode_header(ulong newhead, bool checkVbr = false)
        {
            if ((newhead & (1 << 20)) != 0)
            {
                fr.lsf = (newhead & (1 << 19)) != 0 ? 0x0 : 0x1;
                fr.mpeg25 = 0;
            }
            else {
                fr.lsf = 1;
                fr.mpeg25 = 1;
            }

            if (!Args.TryResync || oldhead == 0)
            {
                /* If "tryresync" is true, assume that certain
                   parameters do not change within the stream! */
                fr.lay = 4 - (long)((newhead >> 17) & 3);

                //if( ((newhead>>12)&0xf) == 0xf)
                //    return 0;

                if (((newhead >> 10) & 0x3) == 0x3)
                    return false;

                if (fr.mpeg25 != 0)
                    fr.sampling_frequency = 6 + (long)((newhead >> 10) & 0x3);
                else
                    fr.sampling_frequency = (long)((newhead >> 10) & 0x3) + (fr.lsf * 3);
                if (fr.sampling_frequency > 8)
                    return false;
                // OLD::: 
                fr.error_protection = (long)((newhead >> 16) & 0x1) ^ 0x1;
            }

            fr.error_protection = ((newhead >> 16) & 0x1) == 0 ? 1 : 0; 
            fr.bitrate_index = (long)((newhead >> 12) & 0xf);
            fr.padding = (long)((newhead >> 9) & 0x1);
            fr.extension = (long)((newhead >> 8) & 0x1);
            fr.mode = (long)((newhead >> 6) & 0x3);
            fr.mode_ext = (long)((newhead >> 4) & 0x3);
            fr.copyright = (long)((newhead >> 3) & 0x1);
            fr.original = (long)((newhead >> 2) & 0x1);
            fr.emphasis = (long)newhead & 0x3;
            fr.stereo = (fr.mode == Constants.MPG_MD_MONO) ? 1 : 2;

            oldhead = newhead;

            if (fr.bitrate_index == 0)
                return false;

            // TODO: Previously, there was subtraction of 4 bytes to framesize no idea why, need to investigate.
            int frameSizeSub = 4;
            //int frameSizeSub = 0;
            fr.WhatLayer = fr.lay;
            switch (fr.lay)
            {
                case 1:
                    fr.jsbound = (fr.mode == Constants.MPG_MD_JOINT_STEREO) ? (fr.mode_ext << 2) + 4 : 32;
                    framesize = Constants.BitRateIndex[fr.lsf, 0, fr.bitrate_index] * 12000;
                    framesize /= (int)Constants.freqs[fr.sampling_frequency];
                    framesize = (int)((framesize + fr.padding) << 2) - frameSizeSub;
                    break;
                case 2:
                    //GetLayer2();
                    fr.jsbound = (fr.mode == Constants.MPG_MD_JOINT_STEREO) ? (fr.mode_ext << 2) + 4 : 0/*fr.II_sblimit*/;
                    framesize = Constants.BitRateIndex[fr.lsf, 1, fr.bitrate_index] * 144000;
                    framesize /= (int)Constants.freqs[fr.sampling_frequency];
                    framesize += (int)(fr.padding - frameSizeSub);
                    //framesize = 144000 * BitRateIndex[fr.lsf, 2, fr.bitrate_index] / (int)(freqs[fr.sampling_frequency] << ((int)fr.lsf)) + (int)fr.padding;
                    break;
                case 3:
                    if (fr.lsf != 0)
                        ssize = (fr.stereo == 1) ? 9 : 17;
                    else
                        ssize = (fr.stereo == 1) ? 17 : 32;
                    if (fr.error_protection != 0)
                        ssize += 2;
                    framesize = Constants.BitRateIndex[fr.lsf, 2, fr.bitrate_index] * 144000;
                    framesize /= (int)(Constants.freqs[fr.sampling_frequency] << ((int)fr.lsf));
                    framesize = (int)(framesize + fr.padding - frameSizeSub);
                    //framesize = 144000 * BitRateIndex[fr.lsf, 2, fr.bitrate_index] / (int)(freqs[fr.sampling_frequency] << ((int)fr.lsf)) + (int)fr.padding;
                    break;
                default:
                    return false;
            }
            
            // Check VBR info.
            if (checkVbr)
            {
                int frameOffset = ssize;
                long offsetNew = _fileStream.Seek(ssize, SeekOrigin.Current);
                ulong data = 0, numFrames = 0, flags = 0, fileLength = 0;
                byte[] hdr = new byte[4];
                head_read(hdr, ref data);

                if (data == Constants.Xing) // Xing - 0x58696E67
                {
                    Args.IsVBR = true; // VBR header.
                    head_read(hdr, ref flags); // Flags
                    if ((flags & 0x00000001) == 1) // Frames Flag
                    {
                        head_read(hdr, ref numFrames); // Frames
                        Args.NumFrames = (uint)numFrames;
                        frameOffset += 4;
                    }
                    if ((flags & 0x00000002) == 2) // Bytes Flag
                    {
                        if (!head_read(hdr, ref fileLength))
                            return false;
                        Args.FileLength = (uint)fileLength;
                        frameOffset += 4;

                    }
                    if ((flags & 0x00000004) == 4) // TOC Flag
                    {
                        byte[] toc = new byte[100];
                        if (_fileStream.Read(toc, 0, 100) != 100)
                            return false;
                        Args.TOC = toc;
                        frameOffset += 100;
                    }

                    if ((flags & 0x00000008) == 8) //VBR Scale Flag
                    {
                    }

                    // Seek to the end of the current frame.
                    _fileStream.Seek(framesize - frameOffset, SeekOrigin.Current);

                    // Read header of the next frame.
                    if (!head_read(hdr, ref data))
                        return false;
                    return decode_header(data);
                }
                else
                    _fileStream.Seek(-1 * ssize, SeekOrigin.Current);
            }

            syncword = newhead & 0xFFF80CC0;
            return true;
        }

        private bool SkipID3v2Tag(byte[] abuf, ref ulong newhead)
        {
            long numread;
            byte[] tbuf = new byte[10];
            tbuf[0] = (byte)(newhead & 0xFF); // Major version.

            // skip minor version(byte), flags(byte) and tag size (4 bytes)
            numread = _fileStream.Read(tbuf, 1, 6);
            if (numread != 6) return false;

            //hbuf = tbuf[3] | (tbuf[2] << 7) | (tbuf[1] << 14) | (tbuf[0] << 21);
            int tagSize = tbuf[6] | (tbuf[5] << 7) | (tbuf[4] << 14) | (tbuf[3] << 21);
            long XPosi = tagSize + 6;

            if(_readTags)
            {
                // Read ID3v2 tags.
                var tagsReader = new ID3v2TagsReader();
                if(tagsReader.ReadTags(_fileStream, tagSize))
                {
                    var track = tagsReader.TagByName("TRCK");
                    if(!string.IsNullOrEmpty(track))
                    {
                        int idx = track.IndexOf("/"); // 1/12
                        if (idx != -1)
                            track = track.Substring(0, idx);
                    }

                    Album = tagsReader.TagByName("TALB");
                    Artist = tagsReader.TagByName("TPE1");
                    Song = tagsReader.TagByName("TIT2");
                    Genre = tagsReader.TagByName("TCON");
                    Composer = tagsReader.TagByName("TCOM");
                    Year = tagsReader.TagByName("TYER");
                    Track = track;
                    _attachedPictures = tagsReader.AttachedPictures;
                }
            }

            _fileStream.Seek(XPosi, SeekOrigin.Begin);
            return head_read(abuf, ref newhead);
        }

        private void Reset_Stream()
        {
            _fileStream.Seek(0, SeekOrigin.Begin);

            fsize = 0; fsizeold = 0; oldhead = 0; firsthead = 0;
            ssize = 0;
            bsbuf = bsspace_1;
            bsnum = 0;
            Args.IsVBR = false;
            Args.NumFrames = 0;
            Args.FileLength = 0;
            Args.TOC = null;
            Args.CurrentPos = 0;
        }

        public void Dispose()
        {
            // if (_fileStream != null)
            //     _fileStream.Dispose();
            _fileStream = null;
        }
    }
}
