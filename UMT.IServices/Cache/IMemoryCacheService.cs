using System;
using System.Runtime.Caching;

namespace UMT.IServices.Cache
{
    public interface IMemoryCacheService : IDisposable
    {
        TOut GetOrAdd<TOut>(string key, TimeSpan slidingExpiration, Func<TOut> acquireFunc, CacheEntryRemovedCallback removedCallback = null);
        bool Exists(string key);
        void Add(string key, TimeSpan slidingExpiration, object obj, CacheEntryRemovedCallback removedCallback = null);
        TOut Get<TOut>(string key);
        void Remove(string key);
        object Get(string key);
        void Clear();
    }
}
