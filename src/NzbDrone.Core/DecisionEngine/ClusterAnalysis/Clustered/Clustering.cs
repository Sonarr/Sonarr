using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Clustered
{
    public class Clustering<TSource> : IGrouping<double, TSource>
    {
        private readonly ISet<TSource> _instances;

        public Clustering(ISet<TSource> instances, Func<TSource, double> clusterValueFunc)
        {
            _instances = instances;

            if (_instances.Count == 0) return;

            AggregateInfo AggregateInstances(AggregateInfo agg, TSource current)
            {
                var currentValue = clusterValueFunc(current);
                agg.Max = Math.Max(agg.Max, currentValue);
                agg.Min = Math.Min(agg.Min, currentValue);
                agg.Count += 1;
                agg.Sum += currentValue;
                return agg;
            }

            var seed = new AggregateInfo {Min = double.MaxValue, Max = double.MinValue, Sum = 0, Count = 0};
            var info = _instances.Aggregate(seed, AggregateInstances);
            Key = info.Sum / info.Count;
        }

        private class AggregateInfo
        {
            public double Max;
            public double Min;
            public double Sum;
            public double Count;
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return _instances.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public double Key { get; }
    }
}
