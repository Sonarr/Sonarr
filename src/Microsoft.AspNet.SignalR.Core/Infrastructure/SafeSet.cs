// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    internal class SafeSet<T>
    {
        private readonly ConcurrentDictionary<T, object> _items;

        public SafeSet()
        {
            _items = new ConcurrentDictionary<T, object>();
        }

        public SafeSet(IEqualityComparer<T> comparer)
        {
            _items = new ConcurrentDictionary<T, object>(comparer);
        }

        public SafeSet(IEnumerable<T> items)
        {
            _items = new ConcurrentDictionary<T, object>(items.Select(x => new KeyValuePair<T, object>(x, null)));
        }

        public ICollection<T> GetSnapshot()
        {
            // The Keys property locks, so Select instead
            return _items.Keys;
        }

        public bool Contains(T item)
        {
            return _items.ContainsKey(item);
        }

        public bool Add(T item)
        {
            return _items.TryAdd(item, null);
        }

        public bool Remove(T item)
        {
            object _;
            return _items.TryRemove(item, out _);
        }

        public bool Any()
        {
            return _items.Any();
        }

        public long Count
        {
            get { return _items.Count; }
        }
    }
}
