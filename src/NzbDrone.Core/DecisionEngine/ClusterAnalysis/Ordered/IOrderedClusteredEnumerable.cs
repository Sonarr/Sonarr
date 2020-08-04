using System;
using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public interface IOrderedClusteredEnumerable<TElement> : IEnumerable<TElement>
    {
        IOrderedClusteredEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer, bool descending);

        IOrderedClusteredEnumerable<TElement> CreateClusterOrderedEnumerable(
            Func<TElement, double> distanceValueSelector, double clusterDistanceCutPoint, bool descending);
    }
}
