using System;
using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public class ClusteredElement<TElement>
    {
        public TElement Element { get; }
        private readonly Dictionary<Guid, double> _clusterValues = new Dictionary<Guid, double>();

        public ClusteredElement(TElement element)
        {
            Element = element;
        }

        public double this[Guid id]
        {
            get => _clusterValues[id];
            set => _clusterValues[id] = value;
        }

    }
}