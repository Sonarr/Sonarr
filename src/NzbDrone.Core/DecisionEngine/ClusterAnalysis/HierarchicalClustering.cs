using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis
{
    public interface IAnalyseClusters<T>
    {
        Cluster<T> Cluster(IEnumerable<T> candidates);
    }

    public sealed class HierarchicalClustering<T> : IAnalyseClusters<T>
    {
        private readonly Func<double, double, double> _linkageFunction;
        private readonly Func<T, T, double> _distanceFunction;
        private double[,] _distanceMatrix;

        public HierarchicalClustering(Func<T, T, double> distanceFunction, Func<double, double, double> linkageFunction)
        {
            _distanceFunction = distanceFunction;
            _linkageFunction = linkageFunction;
        }

        public Cluster<T> Cluster(IEnumerable<T> candidates)
        {
            if(candidates == null) return Cluster<T>.Empty();

            var clusters = candidates.Select((c,i) => new Cluster<T>(null, null, 0, c, (ushort)i)).ToArray();

            if (clusters.Length <= 1) return clusters.SingleOrDefault() ?? Cluster<T>.Empty();

            // 1. construct distance matrix for clusters
            ConstructDistanceMatrix(clusters);

            do
            {
                // 2. find 2 clusters with the smallest distance, and update the distance matrix
                var minClusterPair = FindNearestClusters(clusters);

                // 3. merge the 2 clusters, remove the two merged clusters from the list and append the merged cluster
                clusters = clusters
                    .Where(c => !minClusterPair.ImmediatelyContains(c))
                    .Concat(new[] {minClusterPair})
                    .ToArray();

                // 4. repeat until all clusters are merged (one cluster left in the list, with left and right nodes)
                // alternatively the number of steps is the number of initial clusters - 1 (as the list is reduced by one each iteration)
            } while (clusters.Length > 1 || !clusters.First().IsMerged);

            return clusters.Single();
        }

        private Cluster<T> FindNearestClusters(Cluster<T>[] clusters)
        {
            Cluster<T> minClusterLeft = null;
            Cluster<T> minClusterRight = null;
            var minXIndex = int.MaxValue;
            var minYIndex = int.MaxValue;
            var minDistance = double.MaxValue;

            void DifferentIndexAction(int xIndex, int yIndex)
            {
                var distance = _distanceMatrix[clusters[xIndex].Id, clusters[yIndex].Id];
                
                if (distance >= minDistance) return;

                minClusterLeft = clusters[xIndex];
                minClusterRight = clusters[yIndex];
                minXIndex = xIndex;
                minYIndex = yIndex;
                minDistance = distance;

            }

            ReflectiveMatrixIterate(null, DifferentIndexAction, clusters.Length);

            var minCluster = new Cluster<T>(minClusterLeft, minClusterRight, minDistance, minClusterLeft.Id);
            UpdateDistanceMatrix(clusters, minXIndex, minYIndex, minCluster);

            return minCluster;
        }

        private void ConstructDistanceMatrix(Cluster<T>[] clusters)
        {
            _distanceMatrix = new double[clusters.Length,clusters.Length];

            void DifferentIndexAction(int xIndex, int yIndex)
            {
                var distance = _distanceFunction(clusters[xIndex].Instance, clusters[yIndex].Instance);
                _distanceMatrix[clusters[xIndex].Id, clusters[yIndex].Id] = distance;
                _distanceMatrix[clusters[yIndex].Id, clusters[xIndex].Id] = distance;
            }

            void SameIndexAction(int xIndex, int yIndex)
            {
                _distanceMatrix[clusters[xIndex].Id, clusters[yIndex].Id] = 0;
            }

            ReflectiveMatrixIterate(SameIndexAction, DifferentIndexAction, clusters.Length);

        }

        private void UpdateDistanceMatrix(Cluster<T>[] clusters, int minXIndex, int minYIndex, Cluster<T> minCluster)
        {
            // add new row and column for the merged cluster (which is actually re-using a row/column in the matrix
            // for one of the merged clusters)
            // distance is the max (linkage function) of the distances between each element of the merged cluster
            // and each of the remaining elements

            var minXCluster = clusters[minXIndex];
            var minYCluster = clusters[minYIndex];

            for (var index = 0; index < clusters.Length; index++)
            {
                if(index == minXIndex || index == minYIndex) continue;

                var distanceBetweenMinXAndIndex = _distanceMatrix[minXCluster.Id, clusters[index].Id];
                var distanceBetweenMinYAndIndex = _distanceMatrix[minYCluster.Id, clusters[index].Id];

                var distance = _linkageFunction(distanceBetweenMinXAndIndex, distanceBetweenMinYAndIndex);
                _distanceMatrix[minCluster.Id, clusters[index].Id] = distance;
                _distanceMatrix[clusters[index].Id, minCluster.Id] = distance;
            }
        }

        private static void ReflectiveMatrixIterate(Action<int, int> sameIndexAction, Action<int, int> differentIndexAction, int max)
        {
            var xIndex = 0;
            var yIndex = 0;

            while (yIndex < max)
            {
                if (xIndex == yIndex)
                {
                    sameIndexAction?.Invoke(xIndex, yIndex);
                    xIndex = 0;
                    yIndex += 1;
                    continue;
                }

                differentIndexAction(xIndex, yIndex);
                xIndex += 1;
            }
        }
    }
}
