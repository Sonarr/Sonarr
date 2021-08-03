using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace NzbDrone.Common.Cache
{
    public class CachedDictionary<TValue> : ICachedDictionary<TValue>
    {
        private readonly Func<IDictionary<string, TValue>> _fetchFunc;
        private readonly TimeSpan? _ttl;

        private DateTime _lastRefreshed = DateTime.MinValue;
        private ConcurrentDictionary<string, TValue> _items = new ConcurrentDictionary<string, TValue>();

        public CachedDictionary(Func<IDictionary<string, TValue>> fetchFunc = null, TimeSpan? ttl = null)
        {
            _fetchFunc = fetchFunc;
            _ttl = ttl;
        }

        public bool IsExpired(TimeSpan ttl)
        {
            return _lastRefreshed.Add(ttl) < DateTime.UtcNow;
        }

        public void RefreshIfExpired()
        {
            if (_ttl.HasValue && _fetchFunc != null)
            {
                RefreshIfExpired(_ttl.Value);
            }
        }

        public void RefreshIfExpired(TimeSpan ttl)
    {
            if (IsExpired(ttl))
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (_fetchFunc == null)
            {
                throw new InvalidOperationException("Cannot update cache without data source.");
            }

            Update(_fetchFunc());
            ExtendTTL();
        }

        public void Update(IDictionary<string, TValue> items)
        {
            _items = new ConcurrentDictionary<string, TValue>(items);
            ExtendTTL();
        }

        public void ExtendTTL()
        {
            _lastRefreshed = DateTime.UtcNow;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public ICollection<TValue> Values
        {
            get
            {
                RefreshIfExpired();
                return _items.Values;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Count
        {
            get
            {
                RefreshIfExpired();
                return _items.Count;
            }
        }

        public TValue Get(string key)
        {
            RefreshIfExpired();

            TValue result;

            if (!_items.TryGetValue(key, out result))
            {
                throw new KeyNotFoundException(string.Format("Item {0} not found in cache.", key));
            }

            return result;
        }

        public TValue Find(string key)
        {
            RefreshIfExpired();

            TValue result;

            _items.TryGetValue(key, out result);

            return result;
        }

        public void Clear()
        {
            _items.Clear();
            _lastRefreshed = DateTime.MinValue;
        }

        public void ClearExpired()
        {
            if (!_ttl.HasValue)
            {
                throw new InvalidOperationException("Checking expiry without ttl not possible.");
            }

            if (IsExpired(_ttl.Value))
            {
                Clear();
            }
        }

        public void Remove(string key)
        {
            TValue item;
            _items.TryRemove(key, out item);
        }
    }
}
