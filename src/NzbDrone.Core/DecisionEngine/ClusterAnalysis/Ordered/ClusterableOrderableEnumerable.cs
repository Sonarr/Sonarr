using System;
using System.Collections;
using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public sealed class ClusterableOrderableEnumerable<TElement> : IOrderedClusteredEnumerable<TElement>
    {
        private readonly IEnumerable<TElement> _source;

        public ClusterableOrderableEnumerable(IEnumerable<TElement> source)
        {
            _source = source;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IOrderedClusteredEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return new OrderedEnumerable<TElement, TKey>(_source, null, keySelector, comparer, descending);
        }

        public IOrderedClusteredEnumerable<TElement> CreateClusterOrderedEnumerable(
            Func<TElement, double> distanceValueSelector, double clusterDistanceCutPoint, bool descending)
        {
            return new ClusterOrderedEnumerable<TElement>(_source, null, distanceValueSelector, clusterDistanceCutPoint,
                descending);
        }
    }
}