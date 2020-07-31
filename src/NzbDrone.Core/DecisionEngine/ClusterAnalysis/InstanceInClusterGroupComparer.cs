using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis
{
    public sealed class InstanceInClusterGroupComparer<T> : IComparer<T>
        where T : IEquatable<T>
    {
        private readonly IEnumerable<ClusterGroup<T>> _clusters;

        public InstanceInClusterGroupComparer(IEnumerable<ClusterGroup<T>> clusters)
        {
            _clusters = clusters.ToList();
        }

        public int Compare(T x, T y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null)) return 0;
            if (ReferenceEquals(null, x)) return -1;
            if (ReferenceEquals(y, null)) return 1;

            var xCluster = _clusters.Single(c => c.Contains(x));
            var yCluster = _clusters.Single(c => c.Contains(y));
            return xCluster.CompareTo(yCluster);
        }
    }
}
