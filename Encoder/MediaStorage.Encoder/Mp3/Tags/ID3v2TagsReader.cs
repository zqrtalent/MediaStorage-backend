using System;
using System.Collections.Generic;
using System.IO;
using MediaStorage.IO;
using MediaStorage.Common.Dtos.Encoder;

namespace MediaStorage.Encoder.Mp3.Tags
{
    public enum TextEncodingType
    {
        Ascii = 0,
        UTF16_with_BOM = 1,
        UTF16_without_BOM = 2,
        UTF8 = 3,
    }

    public class ID3v2TagsReader
    {
        public bool ReadTags(IStorageFile file, int headerDataSize)
        {
            //TALB - album
            //TCOM - composer
            //TIT2 - song name.
            //TYER - year
            //TCON - genre
            //TRCK - track number
            //TPE1 - lead performer
            //TPE2 - band
            //APIC - Attached picture

            HashSet<string> setFrameIds = new HashSet<string>(new[] { "TALB", "TCOM", "TIT2", "TYER", "TCON", "TRCK", "TPE1", "TPE2", "APIC" });
            Dictionary<string, string> dicAttributes = new Dictionary<string, string>();
            List<AttachedPicture> attachedPictures = new List<AttachedPicture>();
            int offset = 0, offsetEnd = 0, numread = 0;
            byte[] tbuf = new byte[10];

            while (offset <= headerDataSize)
            {
                // Read frame id(4 bytes), size(4 bytes) and flags (2 bytes).
                numread = file.Read(tbuf, 0, 10);
                if (numread != 10) return false;

                var attrName = ReadAsString(tbuf, 0, 4, out offsetEnd);
                if (string.IsNullOrEmpty(attrName))
                    break;

                int size = (int)(BigEndianRead(tbuf, 4) & 0x7FFFFFFF);
                if (size == 0)
                    break;

                offset += (10 + size);

                if (setFrameIds.Contains(attrName))
                {
                    if (attrName == "APIC") // Attached picture.
                    {
                        var pict = ReadAttachedPicture(file, size);
                        if (pict != null)
                            attachedPictures.Add(pict);
                        else
                            break;
                        // _fileStream.Seek(size, SeekOrigin.Current); // Next attribute.
                    }
                    else
                    {
                        byte[] frameData = new byte[size];
                        numread = file.Read(frameData, 0, size);
                        if (numread != size)
                            return false;
                        dicAttributes[attrName] = ReadAsString(frameData, 1, size - 1, out offsetEnd, frameData[0]);
                    }
                }
                else
                    file.Seek(size, SeekOrigin.Current); // Next attribute.
            }

            _attachedPictures = attachedPictures; // Attached pictures.
            _id3v2Tags = dicAttributes; // Set ID3v2 attributes.
            return true;
        }

        private Dictionary<string, string> _id3v2Tags = new Dictionary<string, string>();
        private IEnumerable<AttachedPicture> _attachedPictures;

        public IEnumerable<AttachedPicture> AttachedPictures => _attachedPictures;

        public string TagByName(string tagName)
        {
            if (_id3v2Tags == null)
                return string.Empty;
            if (_id3v2Tags.ContainsKey(tagName))
                return _id3v2Tags[tagName];
            return string.Empty;
        }

        private AttachedPicture ReadAttachedPicture(IStorageFile file, int size)
        {
            string mimeType = string.Empty, description = string.Empty;
            byte[] attachedPicture = new byte[size];
            int numread = file.Read(attachedPicture, 0, size);
            if (numread != size) return null;

            int offset = 0;
            byte encodingType = attachedPicture[0];
            mimeType = ReadAsString(attachedPicture, 1, -1, out offset, encodingType);
            if (string.IsNullOrEmpty(mimeType))
                return null;

            AttachedPictureType pictureType = (AttachedPictureType)attachedPicture[offset++];
            description = ReadAsString(attachedPicture, offset, -1, out offset, encodingType);

            byte[] pictureData = new byte[size - offset];
            Array.Copy(attachedPicture, offset, pictureData, 0, pictureData.Length);

            return new AttachedPicture()
            {
                MimeType = mimeType,
                PictureData = pictureData,
                Type = pictureType
            };
        }

        private uint BigEndianRead(byte[] buf, int offset)
        {
            uint result = buf[offset];
            result <<= 8;
            result |= buf[offset + 1];
            result <<= 8;
            result |= buf[offset + 2];
            result <<= 8;
            result |= buf[offset + 3];
            return result;
        }

        private string ReadAsString(byte[] buf, int offset, int length, out int offsetEnd, byte encodingType = 0xff)
        {
            offsetEnd = offset;
            string val = string.Empty;
            if (length == -1)
            {
                length = 0;
                // UTF16 [UNICODE] with BOM - Terminated with $00 00
                // UTF16 [UNICODE] without BOM - Terminated with $00 00
                if (encodingType == (byte)TextEncodingType.UTF16_with_BOM || encodingType == (byte)TextEncodingType.UTF16_without_BOM)
                {
                    while ((buf[offset + length] != 0 && buf[offset + length + 1] != 0) || length >= (buf.Length - offset))
                        length += 2;
                }
                else
                {
                    while (buf[offset + length] != 0 || length >= (buf.Length - offset))
                        length++;
                }
            }

            offsetEnd += length;

            if (encodingType == 0xff)
            {
                char[] result = new char[length];
                Array.Copy(buf, offset, result, 0, length);
                val = new string(result);
            }
            else
            {
                switch ((TextEncodingType)encodingType)
                {
                    case TextEncodingType.Ascii: // ASCII Terminated with $00
                        {
                            val = System.Text.ASCIIEncoding.ASCII.GetString(buf, offset, length).Trim().Replace("\x0", string.Empty);
                            offsetEnd++; // ending
                            break;
                        }
                    case TextEncodingType.UTF16_with_BOM: // UTF16 [UNICODE] with BOM - Terminated with $00 00
                    case TextEncodingType.UTF16_without_BOM: // UTF16 [UNICODE] without BOM - Terminated with $00 00
                        {
                            // Remove Unicode string special characters.
                            if (buf[offset] == '\xff' && buf[offset + 1] == '\xfe')
                                val = System.Text.UnicodeEncoding.Unicode.GetString(buf, offset + 2, length - 2);
                            else
                                val = System.Text.UnicodeEncoding.Unicode.GetString(buf, offset, length);
                            val = val.Trim().Replace("\x0", string.Empty);
                            offsetEnd += 2; // ending
                            break;
                        }
                    case TextEncodingType.UTF8: // UTF8 Terminated with $00
                        {
                            val = System.Text.ASCIIEncoding.UTF8.GetString(buf, offset, length).Trim();
                            offsetEnd++; // ending
                            break;
                        }
                }
            }

            return val;
        }
    }
}
