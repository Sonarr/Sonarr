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


        private static Func<TElement, TElement, double> GetDistanceFunction<TElement>(Func<TElement, double> distanceValueSelector)
        {
            return (left, right) => Math.Abs(distanceValueSelector(left) - distanceValueSelector(right));
        }

        public static IOrderedClusteredEnumerable<TElement> Cluster<TElement>(this IEnumerable<TElement> source)
        {
            return new ClusterableOrderableEnumerable<TElement>(source);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderByCluster<TElement>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, double> distanceValueSelector, double clusterDistanceCutPoint)
        {
            return new ClusterOrderedEnumerable<TElement>(source, null, GetDistanceFunction(distanceValueSelector), CompleteLinkage, clusterDistanceCutPoint, distanceValueSelector, false);
        }
        public static IOrderedClusteredEnumerable<TElement> OrderByClusterDescending<TElement>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, double> distanceValueSelector, double clusterDistanceCutPoint)
        {
            return new ClusterOrderedEnumerable<TElement>(source, null, GetDistanceFunction(distanceValueSelector), CompleteLinkage, clusterDistanceCutPoint, distanceValueSelector, true);
        }

        public static IOrderedClusteredEnumerable<TElement> ThenByCluster<TElement>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, double> distanceValueSelector, double clusterDistanceCutPoint)
        {
            return source.CreateClusterOrderedEnumerable(GetDistanceFunction(distanceValueSelector), CompleteLinkage, clusterDistanceCutPoint, distanceValueSelector, false);
        }
        public static IOrderedClusteredEnumerable<TElement> ThenByClusterDescending<TElement>(this IOrderedClusteredEnumerable<TElement> source, Func<TElement, double> distanceValueSelector, double clusterDistanceCutPoint)
        {
            return source.CreateClusterOrderedEnumerable(GetDistanceFunction(distanceValueSelector), CompleteLinkage, clusterDistanceCutPoint, distanceValueSelector, true);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderBy<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source,
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            return new OrderedOrderedClusteredEnumerable<TElement, TKey>(source, null, keySelector, comparer, false);
        }

        public static IOrderedClusteredEnumerable<TElement> OrderByDescending<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source,
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            return new OrderedOrderedClusteredEnumerable<TElement, TKey>(source, null, keySelector, comparer, true);
        }

        public static IOrderedClusteredEnumerable<TElement> ThenBy<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source,
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            return source.CreateOrderedEnumerable(keySelector, comparer, false);
        }

        public static IOrderedClusteredEnumerable<TElement> ThenByDescending<TElement, TKey>(this IOrderedClusteredEnumerable<TElement> source,
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            return source.CreateOrderedEnumerable(keySelector, comparer, true);
        }

    }
}
