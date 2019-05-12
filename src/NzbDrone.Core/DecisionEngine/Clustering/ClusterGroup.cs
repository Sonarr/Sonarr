using System;
using System.Collections;
using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.Clustering
{
    public class ClusterGroup<T> : IComparable<ClusterGroup<T>>, IEnumerable<T>
    {
        private readonly ISet<T> _instances;
        private readonly long _sortingKey;

        public ClusterGroup(ISet<T> instances, long sortingKey)
        {
            _instances = instances;
            _sortingKey = sortingKey;
        }

        public int CompareTo(ClusterGroup<T> other)
        {
            if (other == null) return 1;
            return _sortingKey.CompareTo(other._sortingKey);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _instances.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}