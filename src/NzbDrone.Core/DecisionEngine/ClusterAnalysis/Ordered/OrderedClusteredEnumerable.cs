using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public sealed class OrderedClusteredEnumerable<TElement, TKey> : OrderedClusteredEnumerableBase<TElement>
    {
        private readonly IComparer<TKey> _comparer;
        private readonly bool _descending;


        private readonly Func<IEnumerable<TElement>, IEnumerable<IGrouping<TKey, TElement>>> _groupingFunction;

        public OrderedClusteredEnumerable(IEnumerable<TElement> source, bool descending, IComparer<TKey> comparer, Func<IEnumerable<TElement>, IEnumerable<IGrouping<TKey, TElement>>> groupingFunction)
            : base(source)
        {
            _descending = descending;
            _comparer = comparer;
            _groupingFunction = groupingFunction;
        }

        public override IEnumerable<TElement> Apply(IEnumerable<TElement> source)
        {
            var grouping = _groupingFunction(source).ToArray();
            var ordering = ApplyOrdering(grouping);
            return Next != null ? ordering.SelectMany(g => Next.Apply(g)) : ordering.SelectMany(g => g);
        }

        private IOrderedEnumerable<IGrouping<TKey, TElement>> ApplyOrdering(
            IEnumerable<IGrouping<TKey, TElement>> grouping)
        {
            if (_comparer != null)
            {
                return _descending ? grouping.OrderByDescending(g => g.Key, _comparer) : grouping.OrderBy(g => g.Key, _comparer);
            }
            return _descending ? grouping.OrderByDescending(g => g.Key) : grouping.OrderBy(g => g.Key);
        }
    }
}
