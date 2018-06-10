using System;
using System.Collections.Generic;
using System.Linq;
using MediaStorage.IO;
using MediaStorage.Common.Dtos.Session;
using MediaStorage.Data.Streaming.Entities;
using MediaStorage.Data.Streaming.Context;

namespace MediaStorage.Core.Services
{
    internal class StreamingUserSessionService : IStreamingUserSessionService
    {
        private readonly IStreamingDataContext _dataContext;
        public StreamingUserSessionService(IStreamingDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        /// <summary>
        /// Starts or continues previous media straiming session.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="ip">Ip address</param>
        /// <returns>Streaming session info object, containing playing media and milliseconds playing at.</returns>
        public SessionInfo StartSession(Guid userId, string ip)
        {
            var session = (from ss in _dataContext.Get<StreamingSession>()
                           where ss.UserId == userId
                           select ss).FirstOrDefault();

            if (session == null)
            {
                session = new StreamingSession
                {
                    DateStarted = DateTime.UtcNow,
                    IsActive = true,
                    StateInfoJson = string.Empty,
                    UserId = userId,
                    UserIp = ip
                };
                
                _dataContext.Add(session);
                _dataContext.SaveChanges();
            }
            else
                if (!session.IsActive)
            {
                session.IsActive = true;
                session.DateStarted = DateTime.UtcNow;
                session.DateFinished = null;
                
                _dataContext.Update(session);
                _dataContext.SaveChanges();
            }

            return new SessionInfo
            {
                SessionKey = session.Id.ToString(),
                PlayingMediaId = session.PlayingMediaId?.ToString() ?? string.Empty,
                PlayingAtMSec = (session.PlayingMediaId.HasValue ? session.PlayingAtMSec : 0)
            };
        }

        /// <summary>
        /// Stops media streaming session.
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public bool StopSession(string sessionKey)
        {
            Guid sessionId;
            if (!Guid.TryParse(sessionKey, out sessionId))
                return false;

            var session = (from ss in _dataContext.Get<StreamingSession>()
                           where ss.Id == sessionId
                           select ss).FirstOrDefault();

            if (session != null && session.IsActive)
            {
                session.IsActive = false;
                session.DateFinished = DateTime.UtcNow;
                
                _dataContext.Update(session);
                _dataContext.SaveChanges();
            }
            return true;
        }

        private StreamingSession GetSession(string sessionKey, bool? activeStatus)
        {
            Guid sessionId = Guid.Empty;
            if(!Guid.TryParse(sessionKey, out sessionId))
                throw new ArgumentException($"Invalid argument {nameof(sessionKey)}!");

            if(activeStatus.HasValue)
                return (from ss in _dataContext.Get<StreamingSession>()
                        where ss.Id == sessionId && ss.IsActive == activeStatus.Value
                        select ss).FirstOrDefault();
            return (from ss in _dataContext.Get<StreamingSession>() where ss.Id == sessionId select ss).FirstOrDefault();
        }

        public bool GetSessionInfo(string sessionKey, out Guid playingMediaId, out int playingAtMSec)
        {
            var session = GetSession(sessionKey, true);
            playingMediaId = session?.PlayingMediaId ?? Guid.Empty;
            playingAtMSec = session?.PlayingAtMSec ?? 0;
            return session != null;
        }
        
        public bool UpdateSessionInfo(string sessionKey, Guid playingMediaId, int playingAtMSec)
        {
            var session = GetSession(sessionKey, true);
            if(session != null)
            {
                session.PlayingAtMSec = playingAtMSec;
                session.PlayingMediaId = playingMediaId;
                _dataContext.Update(session);
                _dataContext.SaveChanges();
                return true;
            }
            return false;
        }

        // Increases media playing duration in milliseconds.
        public bool IncreaseMediaPlayingDuration(string sessionKey, int increaseDurationMSec)
        {
            // var session = GetSession(sessionKey, true);
            // if(session != null)
            // {
            //     return true;
            // }
            return false;
        }
    }
}
