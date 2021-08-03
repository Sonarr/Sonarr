using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;

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
        private readonly TimeSpan? _defaultLifeTime;
        private readonly bool _rollingExpiry;

        public Cached(TimeSpan? defaultLifeTime = null, bool rollingExpiry = false)
        {
            _store = new ConcurrentDictionary<string, CacheItem>();
            _defaultLifeTime = defaultLifeTime;
            _rollingExpiry = rollingExpiry;
        }

        public void Set(string key, T value, TimeSpan? lifeTime = null)
        {
            Ensure.That(key, () => key).IsNotNullOrWhiteSpace();
            _store[key] = new CacheItem(value, lifeTime ?? _defaultLifeTime);
        }

        public T Find(string key)
        {
            CacheItem cacheItem;
            if (!_store.TryGetValue(key, out cacheItem))
            {
                return default(T);
            }

            if (cacheItem.IsExpired())
            {
                if (TryRemove(key, cacheItem))
                {
                    return default(T);
                }

                if (!_store.TryGetValue(key, out cacheItem))
                {
                    return default(T);
                }
            }

            if (_rollingExpiry && _defaultLifeTime.HasValue)
            {
                _store.TryUpdate(key, new CacheItem(cacheItem.Object, _defaultLifeTime.Value), cacheItem);
            }

            return cacheItem.Object;
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

            lifeTime = lifeTime ?? _defaultLifeTime;

            CacheItem cacheItem;

            if (_store.TryGetValue(key, out cacheItem) && !cacheItem.IsExpired())
            {
                if (_rollingExpiry && lifeTime.HasValue)
                {
                    _store.TryUpdate(key, new CacheItem(cacheItem.Object, lifeTime), cacheItem);
                }
            }
            else
            {
                var newCacheItem = new CacheItem(function(), lifeTime);
                if (cacheItem != null && _store.TryUpdate(key, newCacheItem, cacheItem))
                {
                    cacheItem = newCacheItem;
                }
                else
                {
                    cacheItem = _store.GetOrAdd(key, newCacheItem);
                }
            }

            return cacheItem.Object;
        }

        public void Clear()
        {
            _store.Clear();
        }

        public void ClearExpired()
        {
            var collection = (ICollection<KeyValuePair<string, CacheItem>>)_store;

            foreach (var cached in _store.Where(c => c.Value.IsExpired()).ToList())
            {
                collection.Remove(cached);
            }
        }

        public ICollection<T> Values
        {
            get
            {
                return _store.Values.Select(c => c.Object).ToList();
            }
        }

        private bool TryRemove(string key, CacheItem value)
        {
            var collection = (ICollection<KeyValuePair<string, CacheItem>>)_store;

            return collection.Remove(new KeyValuePair<string, CacheItem>(key, value));
        }
    }
}
