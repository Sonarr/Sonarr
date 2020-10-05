using System;
using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public interface IOrderedClusteredEnumerable<TElement> : IEnumerable<TElement>
    {
        IOrderedClusteredEnumerable<TElement> CreateOrderedGroupedEnumerable<TKey>(Func<TElement, TKey> keySelector,
            bool descending, IComparer<TKey> comparer);

        IOrderedClusteredEnumerable<TElement> CreateOrderedClusteredEnumerable<TKey>(
            Func<TElement, double> keySelector, bool descending, double distanceCutPoint);

    }
}
