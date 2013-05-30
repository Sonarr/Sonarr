using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common.Cache
{
    public class Cached<T> : ICached<T>
    {
        private readonly ConcurrentDictionary<string, T> _store;

        public Cached()
        {
            _store = new ConcurrentDictionary<string, T>();
        }

        public void Set(string key, T value)
        {
            Ensure.That(() => key).IsNotNullOrWhiteSpace();
            _store[key] = value;
        }

        public T Get(string key)
        {
            return Get(key, () => { throw new KeyNotFoundException(key); });
        }

        public T Find(string key)
        {
            T value;
            _store.TryGetValue(key, out value);
            return value;
        }

        public T Get(string key, Func<T> function)
        {
            Ensure.That(() => key).IsNotNullOrWhiteSpace();

            T value;

            if (!_store.TryGetValue(key, out value))
            {
                value = function();
                Set(key, value);
            }

            return value;
        }

        public bool ContainsKey(string key)
        {
            Ensure.That(() => key).IsNotNullOrWhiteSpace();
            return _store.ContainsKey(key);
        }

        public void Clear()
        {
            _store.Clear();
        }

        public void Remove(string key)
        {
            Ensure.That(() => key).IsNotNullOrWhiteSpace();
            T value;
            _store.TryRemove(key, out value);
        }
    }
}