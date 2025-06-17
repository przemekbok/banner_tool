using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using UMT.IServices.Cache;
using Unity;

namespace UMT.Services.Cache
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private static ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public TOut GetOrAdd<TOut>(string key, TimeSpan slidingExpiration, Func<TOut> acquireFunc, CacheEntryRemovedCallback removedCallback = null)
        {
            if (Exists(key))
            {
                return (TOut)MemoryCache.Default.Get(key);
            }

            lock (_locks.GetOrAdd(key, new object()))
            {
                if (Exists(key))
                {
                    return (TOut)MemoryCache.Default.Get(key);
                }

                TOut obj = acquireFunc();

                Add(key, slidingExpiration, obj, removedCallback);

                return obj;
            }
        }

        public bool Exists(string key)
        {
            return MemoryCache.Default.Contains(key);
        }

        public void Add(string key, TimeSpan slidingExpiration, object obj, CacheEntryRemovedCallback removedCallback = null)
        {
            MemoryCache.Default.Add(key, obj, new CacheItemPolicy { SlidingExpiration = slidingExpiration, RemovedCallback = removedCallback });
        }

        public TOut Get<TOut>(string key)
        {
            return (TOut)MemoryCache.Default.Get(key);
        }

        public object Get(string key)
        {
            return MemoryCache.Default.Get(key);
        }

        public void Remove(string key)
        {
            MemoryCache.Default.Remove(key);
        }

        public void Clear()
        {
            MemoryCache.Default.Trim(100);
        }

        public void Dispose()
        {
            MemoryCache.Default.Dispose();
        }
    }
}
