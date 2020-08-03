using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine.ClusterAnalysis.Clustered;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public sealed class ClusterOrderedEnumerable<TElement> : OrderedClusteredEnumerableBase<TElement>
    {
        private readonly Func<ClusteredElement<TElement>, ClusteredElement<TElement>, double> _distanceFunction;
        private readonly Func<double, double, double> _linkageFunction;
        private readonly double _clusterDistanceCutPoint;
        private readonly Func<TElement, double> _clusterValueFunc;
        private readonly bool _descending;
        private readonly Guid _id = Guid.NewGuid();
        private readonly Func<ClusteredElement<TElement>, double> _keySelector;

        public ClusterOrderedEnumerable(IEnumerable<TElement> source, OrderedClusteredEnumerableBase<TElement> parent,
            Func<TElement, TElement, double> distanceFunction, Func<double, double, double> linkageFunction,
            double clusterDistanceCutPoint, Func<TElement, double> clusterValueFunc, bool descending) : base(source, parent)
        {
            _distanceFunction = (ce1, ce2) => distanceFunction(ce1.Element, ce2.Element);
            _linkageFunction = linkageFunction;
            _clusterDistanceCutPoint = clusterDistanceCutPoint;
            _clusterValueFunc = clusterValueFunc;
            _descending = descending;
            _keySelector = ce => ce[_id];
        }

        public override IOrderedEnumerable<ClusteredElement<TElement>> ApplyOrdering(
            IEnumerable<ClusteredElement<TElement>> source)
        {
            if (Parent == null)
            {
                return _descending ? source.OrderByDescending(_keySelector) : source.OrderBy(_keySelector);
            }

            var parentOrdering = Parent?.ApplyOrdering(source);
            return _descending ? parentOrdering.ThenByDescending(_keySelector) : parentOrdering.ThenBy(_keySelector);
        }

        private IEnumerable<ClusteredElement<TElement>> Cluster(IEnumerable<ClusteredElement<TElement>> source)
        {
            var hca = new HierarchicalClustering<ClusteredElement<TElement>>(_distanceFunction, _linkageFunction);
            var results = hca.Cluster(source);
            var clusters = results.GetClusteredInstances(_clusterDistanceCutPoint);
            foreach (var cluster in clusters)
            {
                ApplyClusterAverage(cluster);
            }

            return clusters.SelectMany(cluster => cluster);
        }

        private void ApplyClusterAverage(ISet<ClusteredElement<TElement>> cluster)
        {
            var avg = cluster.Average(ce => _clusterValueFunc(ce.Element));
            foreach (var clusteredElement in cluster)
            {
                clusteredElement[_id] = avg;
            }
        }

        public override IEnumerable<ClusteredElement<TElement>> ApplyClusterings(IEnumerable<ClusteredElement<TElement>> source)
        {
            if (Parent == null)
            {
                return Cluster(source);
            }

            var parentClustering = Parent?.ApplyClusterings(source);
            return Cluster(parentClustering);
        }
    }
}