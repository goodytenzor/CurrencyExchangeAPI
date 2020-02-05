using CurExApi.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CurExApi.Services
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeTimeLive);

        Task<string> GetCachedResponseAsync(string cacheKey);
    }
}
