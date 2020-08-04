using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine.ClusterAnalysis.Clustered;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public sealed class ClusterOrderedEnumerable<TElement> : OrderedClusteredEnumerableBase<TElement>
    {
        private readonly Func<TElement, double> _distanceValueSelector;
        private readonly double _clusterDistanceCutPoint;
        private readonly bool _descending;
        private readonly Guid _id = Guid.NewGuid();
        private readonly Func<ClusteredElement<TElement>, double> _keySelector;

        public ClusterOrderedEnumerable(IEnumerable<TElement> source, OrderedClusteredEnumerableBase<TElement> parent,
            Func<TElement, double> distanceValueSelector,
            double clusterDistanceCutPoint, bool descending) : base(source, parent)
        {
            _distanceValueSelector = distanceValueSelector;
            _clusterDistanceCutPoint = clusterDistanceCutPoint;
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

            var parentOrdering = Parent.ApplyOrdering(source);
            return _descending ? parentOrdering.ThenByDescending(_keySelector) : parentOrdering.ThenBy(_keySelector);
        }

        private IEnumerable<ClusteredElement<TElement>> Cluster(IEnumerable<ClusteredElement<TElement>> source)
        {
            var clusters = source
                .ClusterBy(ce => _distanceValueSelector(ce.Element), _clusterDistanceCutPoint);

            return ApplyClusterAverage(clusters);
        }

        private IEnumerable<ClusteredElement<TElement>> ApplyClusterAverage(IEnumerable<Clustering<ClusteredElement<TElement>>> clusters)
        {
            var results = new List<ClusteredElement<TElement>>();
            foreach (var cluster in clusters)
            foreach (var clusteredElement in cluster)
            {
                clusteredElement[_id] = cluster.AverageValue;
                results.Add(clusteredElement);
            }

            return results;
        }

        public override IEnumerable<ClusteredElement<TElement>> ApplyClustering(IEnumerable<ClusteredElement<TElement>> source)
        {
            if (Parent == null)
            {
                return Cluster(source);
            }

            var parentClustering = Parent.ApplyClustering(source);
            return Cluster(parentClustering);
        }
    }
}