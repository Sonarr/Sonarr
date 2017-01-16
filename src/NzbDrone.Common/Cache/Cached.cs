using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common.Cache
{

    public class Cached<T> : ICached<T>
    {
        private class CacheItem
        {
            public T Object { get; private set; }
            public DateTime? ExpiryTime { get; private set; }

            public CacheItem(T obj, TimeSpan? lifetime = null)
            {
                Object = obj;
                if (lifetime.HasValue)
                {
                    ExpiryTime = DateTime.UtcNow + lifetime.Value;
                }
            }

            public bool IsExpired()
            {
                return ExpiryTime.HasValue && ExpiryTime.Value < DateTime.UtcNow;
            }
        }

        private readonly ConcurrentDictionary<string, CacheItem> _store;

        public Cached()
        {
            _store = new ConcurrentDictionary<string, CacheItem>();
        }

        public void Set(string key, T value, TimeSpan? lifetime = null)
        {
            Ensure.That(key, () => key).IsNotNullOrWhiteSpace();
            _store[key] = new CacheItem(value, lifetime);
        }

        public T Find(string key)
        {
            CacheItem value;
            _store.TryGetValue(key, out value);

            if (value == null)
            {
                return default(T);
            }

            if (value.IsExpired())
            {
                _store.TryRemove(key, out value);
                return default(T);
            }

            return value.Object;
        }

        public void Remove(string key)
        {
            CacheItem value;
            _store.TryRemove(key, out value);
        }

        public int Count => _store.Count;

        public T Get(string key, Func<T> function, TimeSpan? lifeTime = null)
        {
            Ensure.That(key, () => key).IsNotNullOrWhiteSpace();

            CacheItem cacheItem;
            T value;

            if (!_store.TryGetValue(key, out cacheItem) || cacheItem.IsExpired())
            {
                value = function();
                Set(key, value, lifeTime);
            }
            else
            {
                value = cacheItem.Object;
            }

            return value;
        }

        public void Clear()
        {
            _store.Clear();
        }

        public void ClearExpired()
        {
            foreach (var cached in _store.Where(c => c.Value.IsExpired()))
            {
                Remove(cached.Key);
            }
        }

        public ICollection<T> Values
        {
            get
            {
                return _store.Values.Select(c => c.Object).ToList();
            }
        }

    }
}