using System;
using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine.ClusterAnalysis.Clustered;
using NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis
{
    public static class ClusterExtensions
    {
        public static readonly Func<double, double, double> CompleteLinkage = Math.Max;
        public static readonly Func<double, double, double> SingleLinkage = Math.Min;

        public static IClusteredEnumerable<TElement> ClusterBy<TElement>(this IEnumerable<TElement> source, Func<TElement, double> distanceValueSelector, double clusterDistanceCutPoint)
        {
            double DistanceFunction(TElement left, TElement right) => Math.Abs(distanceValueSelector(left) - distanceValueSelector(right));

            return new ClusteredEnumerable<TElement>(source, DistanceFunction, CompleteLinkage, clusterDistanceCutPoint, distanceValueSelector);
        }

        public static IOrderedClusteredEnumerable<TElement> Cluster<TElement>(this IEnumerable<TElement> source)
        {
            return new OrderedClusteredEnumerableBase<TElement>(source);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderBy<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, TKey> keySelector)
        {
            return Cluster(source).CreateOrderedGroupedEnumerable(keySelector, false, null);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderByDescending<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, TKey> keySelector)
        {
            return Cluster(source).CreateOrderedGroupedEnumerable(keySelector, true, null);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderBy<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            return Cluster(source).CreateOrderedGroupedEnumerable(keySelector, false, comparer);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderByDescending<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            return Cluster(source).CreateOrderedGroupedEnumerable(keySelector, true, comparer);
        }

        public static IOrderedClusteredEnumerable<TElement> ThenBy<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, TKey> keySelector)
        {
            return source.CreateOrderedGroupedEnumerable(keySelector, false, null);
        }

        public static IOrderedClusteredEnumerable<TElement> ThenByDescending<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, TKey> keySelector)
        {
            return source.CreateOrderedGroupedEnumerable(keySelector, true, null);
        }

        public static IOrderedClusteredEnumerable<TElement> ThenBy<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            return source.CreateOrderedGroupedEnumerable(keySelector, false, comparer);
        }

        public static IOrderedClusteredEnumerable<TElement> ThenByDescending<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            return source.CreateOrderedGroupedEnumerable(keySelector, true, comparer);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderByCluster<TElement>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, double> keySelector, double distanceCutPoint)
        {
            return Cluster(source).CreateOrderedClusteredEnumerable<TElement>(keySelector, false, distanceCutPoint);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderByClusterDescending<TElement>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, double> keySelector, double distanceCutPoint)
        {
            return Cluster(source).CreateOrderedClusteredEnumerable<TElement>(keySelector, true, distanceCutPoint);
        }
        public static IOrderedClusteredEnumerable<TElement> ThenByCluster<TElement>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, double> keySelector, double distanceCutPoint)
        {
            return source.CreateOrderedClusteredEnumerable<TElement>(keySelector, false, distanceCutPoint);
        }

        public static IOrderedClusteredEnumerable<TElement> ThenByClusterDescending<TElement>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, double> keySelector, double distanceCutPoint)
        {
            return source.CreateOrderedClusteredEnumerable<TElement>(keySelector, true, distanceCutPoint);
        }
    }
}
