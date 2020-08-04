using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public abstract class OrderedClusteredEnumerableBase<TElement> : IOrderedClusteredEnumerable<TElement>
    {
        protected readonly OrderedClusteredEnumerableBase<TElement> Parent;
        private readonly IEnumerable<TElement> _source;

        protected OrderedClusteredEnumerableBase(IEnumerable<TElement> source, OrderedClusteredEnumerableBase<TElement> parent)
        {
            _source = source;
            Parent = parent;
        }

        public abstract IOrderedEnumerable<ClusteredElement<TElement>> ApplyOrdering(IEnumerable<ClusteredElement<TElement>> source);
        public abstract IEnumerable<ClusteredElement<TElement>> ApplyClustering(IEnumerable<ClusteredElement<TElement>> source);


        public IEnumerator<TElement> GetEnumerator()
        {
            var clusters = ApplyClustering(_source.Select(e => new ClusteredElement<TElement>(e)));
            return ApplyOrdering(clusters).Select(ce => ce.Element).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IOrderedClusteredEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return new OrderedEnumerable<TElement,TKey>(_source, this, keySelector, comparer, descending);
        }

        public IOrderedClusteredEnumerable<TElement> CreateClusterOrderedEnumerable(Func<TElement, double> distanceValueSelector, double clusterDistanceCutPoint, bool descending)
        {
            return new ClusterOrderedEnumerable<TElement>(_source, this, distanceValueSelector, clusterDistanceCutPoint, descending);
        }
    }
}