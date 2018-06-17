using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaStorage.IO;
using MediaStorage.Data.Media.Context;
using MediaStorage.Common.Dtos.Audio;
using MediaStorage.Data.Streaming.Entities;
using MediaStorage.Data.Media.Entities;
using MediaStorage.Encoder.Extensions;
using MediaStorage.Encoder;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace MediaStorage.Core.Services
{
    internal class MediaStreamingService : IMediaStreamingService
    {
        private readonly IStreamingUserSessionService _userSession;
        private readonly IMediaDataContext _mediaDataContext;
        private readonly IStorage _storage;
        private readonly IStreamingUserCacheService _cacheService;

        public MediaStreamingService(IStreamingUserSessionService userSession, IStorage mediaStorage, IMediaDataContext dataContext, IStreamingUserCacheService cacheService)
        {
            _userSession = userSession;
            _mediaDataContext = dataContext;
            _storage = mediaStorage;
            _cacheService = cacheService;
        }
       
        /// <summary>
        /// Checks streaming session and reads packets by packet offset.
        /// </summary>
        /// <param name="sessionKey">Streaming session key</param>
        /// <param name="mediaId">Media identifier</param>
        /// <param name="offset">Media packet offset index.</param>
        /// <param name="numPackets">Number of packets to read.</param>
        /// <returns></returns>
        public AudioPackets ReadAudioPacketsByOffset(string sessionKey, string songId, int offset, int numPackets)
        {
            return ReadMediaPackets(sessionKey, songId, (encoder) => encoder.ReadPackets(offset, numPackets));
        }

        /// <summary>
        /// Checks streaming session and reads packets by time.
        /// </summary>
        /// <param name="sessionKey">Streaming session key</param>
        /// <param name="mediaId">Media identifier</param>
        /// <param name="msecOffset">Milliseconds offset to read packets from.</param>
        /// <param name="numPackets">Number of packets to read.</param>
        /// <returns></returns>
        public AudioPackets ReadAudioPacketsByTime(string sessionKey, string songId, int msecOffset, int numPackets)
        {
            return ReadMediaPackets(sessionKey, songId, (encoder) => encoder.ReadPacketsByTime((uint)msecOffset, numPackets));
        }

        private string ReadMediaFileEncoderStateJson(MediaFile mediaFile, out string mediaFileStateJson)
        {
             var fileStateInfosJson = (from ms in _mediaDataContext.Get<MediaFileStateInfo>()
                                    where ms.MediaFileId == mediaFile.Id
                                    select ms.StateInfoJson).SingleOrDefault();

            string encoderStateInfoJson = string.Empty;
            if(!string.IsNullOrEmpty(fileStateInfosJson))
            {
                var fileStateInfos = JsonConvert.DeserializeObject<MediaFileStateInfos>(fileStateInfosJson);
                using(var encoderFile = _storage.OpenAndRestoreState($"{mediaFile.Url}.enc.json", 
                    fileStateInfos.EncoderFileStateJson))
                {
                    encoderStateInfoJson = System.Text.Encoding.UTF8.GetString(encoderFile.ReadAllBytes());
                }

                mediaFileStateJson = fileStateInfos.MediaFileStateJson;
            }
            else
            {
                mediaFileStateJson = string.Empty;
            }

            return encoderStateInfoJson;
        }
        
        /// <summary>
        /// Reads media packets passing delegate to read packets using different logic.
        /// </summary>
        /// <param name="sessionKey">Streaming session key.</param>
        /// <param name="mediaId">Media identifier.</param>
        /// <param name="funcReadPackets">Delegate function to read packets.</param>
        /// <returns></returns>
        protected AudioPackets ReadMediaPackets(string sessionKey, string songId, Func<IMediaEncoder, AudioPackets> funcReadPackets)
        {
            Guid gSongId, sessionId;
            if (!Guid.TryParse(songId, out gSongId) || !Guid.TryParse(sessionKey, out sessionId))
                return null;

            var mediaFile = (from s in _mediaDataContext.Get<Song>()
                             where s.Id == gSongId && s.Media.IsArchived == false
                             select s.Media).FirstOrDefault();
            if (mediaFile == null)
                return null;

            Guid playingMediaId = Guid.Empty;
            int playingAtMSec = 0;
            if(!_userSession.GetSessionInfo(sessionKey, out playingMediaId, out playingAtMSec))
            {
                throw new InvalidOperationException("Session not exists !");
            }

            string encoderStateJson = string.Empty, mediaFileStateJson = string.Empty;
            AudioPackets packets = null;
            int currentMSec = 0;

            // Check media of state json.
            if(playingMediaId == gSongId)
            {
                mediaFileStateJson = _cacheService.ReadFileState<string>(sessionKey);
                encoderStateJson =  _cacheService.ReadEncoderState<string>(sessionKey);

                if(string.IsNullOrEmpty(encoderStateJson))
                {
                    encoderStateJson = ReadMediaFileEncoderStateJson(mediaFile, out mediaFileStateJson);
                    _cacheService.SaveEncoderState(sessionKey, encoderStateJson);
                }
            }
            else
            {
                encoderStateJson = ReadMediaFileEncoderStateJson(mediaFile, out mediaFileStateJson);
                _cacheService.SaveEncoderState(sessionKey, encoderStateJson);
                _cacheService.SaveFileState(sessionKey, mediaFileStateJson);
            }

            using (var media =  string.IsNullOrEmpty(mediaFileStateJson) ? 
                                _storage.Open(mediaFile.Url) : 
                                _storage.OpenAndRestoreState(mediaFile.Url, mediaFileStateJson))
            {
                IMediaEncoder encoder = MediaEncoderExtension.EncoderByMediaType(mediaFile.Format);
                if (encoder == null)
                    throw new InvalidOperationException($"Unable to instantiate encoder by format = '{mediaFile.Format}'!");

                if (encoder.Init(media, false, encoderStateJson))
                {
                    packets = funcReadPackets.Invoke(encoder);
                    if (packets != null)
                    {
                        // if(encoder.SaveStateIntoJson(out encoderStateJson))
                        // {
                        //     // Save encoder state in cache.
                        //     SaveEncoderStateInCache(sessionKey, encoderStateJson);
                        // }

                        currentMSec = (int)encoder.CurrentMs;
                    }
                }
                
                // Save media file state in cache.
                if(string.IsNullOrEmpty(mediaFileStateJson))
                    _cacheService.SaveFileState(sessionKey, media.SaveStateAsJson());

                // ???
                //encoder.Dispose();
            }

            // Update playing session info.
            _userSession.UpdateSessionInfo(sessionKey, gSongId, currentMSec);
            return packets;
        }
    }
}
