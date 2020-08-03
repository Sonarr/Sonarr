using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Clustered
{
    public interface IClusteredEnumerable<TElement> : IEnumerable<Clustering<TElement>>
    {

    }

    public class ClusteredEnumerable<TElement> : IClusteredEnumerable<TElement>
    {
        private readonly IEnumerable<TElement> _source;
        private readonly Func<TElement, TElement, double> _distanceFunction;
        private readonly Func<double, double, double> _linkageFunction;
        private readonly double _clusterDistanceCutPoint;
        private readonly Func<TElement, double> _clusterValueFunc;

        public ClusteredEnumerable(IEnumerable<TElement> source, Func<TElement, TElement, double> distanceFunction, Func<double, double, double> linkageFunction, double clusterDistanceCutPoint, Func<TElement, double> clusterValueFunc)
        {
            _source = source;
            _distanceFunction = distanceFunction;
            _linkageFunction = linkageFunction;
            _clusterDistanceCutPoint = clusterDistanceCutPoint;
            _clusterValueFunc = clusterValueFunc;
        }

        public IEnumerator<Clustering<TElement>> GetEnumerator()
        {
            var hca = new HierarchicalClustering<TElement>(_distanceFunction, _linkageFunction);
            var results = hca.Cluster(_source);
            var clusters = results.GetClusteredInstances(_clusterDistanceCutPoint);
            return clusters
                .Select(cluster => new Clustering<TElement>(cluster, _clusterValueFunc))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
