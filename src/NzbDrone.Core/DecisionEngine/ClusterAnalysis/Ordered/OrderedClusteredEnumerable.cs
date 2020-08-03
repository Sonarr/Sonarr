using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public sealed class OrderedOrderedClusteredEnumerable<TElement, TKey> : OrderedClusteredEnumerableBase<TElement>
    {
        private readonly Func<ClusteredElement<TElement>, TKey> _keySelector;
        private readonly IComparer<TKey> _comparer;
        private readonly bool _descending;

        public OrderedOrderedClusteredEnumerable(IEnumerable<TElement> source, OrderedClusteredEnumerableBase<TElement> parent,
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) : base(
            source, parent)
        {
            _keySelector = ce => keySelector(ce.Element);
            _comparer = comparer;
            _descending = descending;
        }

        public override IOrderedEnumerable<ClusteredElement<TElement>> ApplyOrdering(IEnumerable<ClusteredElement<TElement>> source)
        {
            if (Parent == null)
            {
                return _descending ? source.OrderByDescending(_keySelector, _comparer) : source.OrderBy(_keySelector, _comparer);
            }

            var parentOrdering = Parent?.ApplyOrdering(source);
            return _descending ? parentOrdering.ThenByDescending(_keySelector, _comparer) : parentOrdering.ThenBy(_keySelector, _comparer);
        }

        public override IEnumerable<ClusteredElement<TElement>> ApplyClusterings(IEnumerable<ClusteredElement<TElement>> source)
        {
            return Parent == null ? source : Parent?.ApplyClusterings(source);
        }
    }
}