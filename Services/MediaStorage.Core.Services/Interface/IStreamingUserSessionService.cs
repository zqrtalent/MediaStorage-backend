using System;
using MediaStorage.Common.Dtos.Session;

namespace MediaStorage.Core.Services
{
    public interface IStreamingUserSessionService
    {
        SessionInfo StartSession(Guid userId, string ip);

        bool StopSession(string sessionKey);

        bool GetSessionInfo(string sessionKey, out Guid playingMediaId, out int playingAtMSec);
        
        bool UpdateSessionInfo(string sessionKey, Guid playingMediaId, int playingAtMSec);

        // Increases media playing duration in milliseconds.
        bool IncreaseMediaPlayingDuration(string sessionKey, int increaseDurationMSec);
    }
}