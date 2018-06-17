using System;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace MediaStorage.Core.Services
{
    internal class StreamingUserCacheService : IStreamingUserCacheService
    {
        private readonly IMemoryCache _cacheService;
        public StreamingUserCacheService(IMemoryCache cacheService)
        {
            _cacheService = cacheService;
        }

        public T ReadFileState<T>(string sessionKey) where T: class
        {
            var stateJson = _cacheService.Get<string>($"{sessionKey}-mediafilestate");
            if(typeof(T) == typeof(string))
                return stateJson as T;
            return JsonConvert.DeserializeObject<T>(stateJson);
        }

        public void SaveFileState<T>(string sessionKey, T state) where T : class
        {
            string stateJson = typeof(T) == typeof(string) ? state as string : JsonConvert.SerializeObject(state);
            _cacheService.Set($"{sessionKey}-mediafilestate", stateJson);
        }

        public T ReadEncoderState<T>(string sessionKey) where T: class
        {
            var stateJson = _cacheService.Get<string>($"{sessionKey}-encoderstate");
            if(typeof(T) == typeof(string))
                return stateJson as T;
            return JsonConvert.DeserializeObject<T>(stateJson);
        }

        public void SaveEncoderState<T>(string sessionKey, T state) where T : class
        {
            string stateJson = typeof(T) == typeof(string) ? state as string : JsonConvert.SerializeObject(state);
            _cacheService.Set($"{sessionKey}-encoderstate", stateJson);
        }
    }
}
