using System;

namespace MediaStorage.Core.Services
{
    public interface IStreamingUserCacheService
    {
        T ReadFileState<T>(string sessionKey) where T: class;
        void SaveFileState<T>(string sessionKey, T state) where T : class;
        T ReadEncoderState<T>(string sessionKey) where T: class;
        void SaveEncoderState<T>(string sessionKey, T state) where T : class;

        /*
        private T ReadFileStateFromCache<T>(string sessionKey) where T: class
        {
            var stateJson = _cacheService.Get<string>($"{sessionKey}-mediafilestate");
            if(typeof(T) == typeof(string))
                return stateJson as T;
            return JsonConvert.DeserializeObject<T>(stateJson);
        }

        private void SaveFileStateInCache<T>(string sessionKey, T state) where T : class
        {
            string stateJson = typeof(T) == typeof(string) ? state as string : JsonConvert.SerializeObject(state);
            _cacheService.Set($"{sessionKey}-mediafilestate", stateJson);
        }

        private T ReadEncoderStateFromCache<T>(string sessionKey) where T: class
        {
            var stateJson = _cacheService.Get<string>($"{sessionKey}-encoderstate");
            if(typeof(T) == typeof(string))
                return stateJson as T;
            return JsonConvert.DeserializeObject<T>(stateJson);
        }

        private void SaveEncoderStateInCache<T>(string sessionKey, T state) where T : class
        {
            string stateJson = typeof(T) == typeof(string) ? state as string : JsonConvert.SerializeObject(state);
            _cacheService.Set($"{sessionKey}-encoderstate", stateJson);
        }
         */

    }
}
