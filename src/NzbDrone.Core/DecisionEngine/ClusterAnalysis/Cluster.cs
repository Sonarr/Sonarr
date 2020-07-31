using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis
{
    public class Cluster<T> : IEnumerable<T>
    {
        private readonly Cluster<T> _left;
        private readonly Cluster<T> _right;
        
        public readonly T Instance;
        public readonly int Id;
        public readonly double Distance;

        public static Cluster<T> Empty() => new Cluster<T>();

        public IEnumerable<T> Contents
        {
            get
            {
                if (_left == null && _right == null && Instance == null) return new List<T>();
                if (_left == null && _right == null) return new List<T> {Instance};

                var leftContents = _left?.Contents ?? new List<T>();
                var rightContents = _right?.Contents ?? new List<T>();
                
                return leftContents.Concat(rightContents);
            }
        }

        private Cluster() : this(null, null, 0, default(T), 0)
        {

        }

        public Cluster(Cluster<T> left, Cluster<T> right, double distance, int id)
            : this(left, right, distance, default(T), id)
        {
            
        }

        public Cluster(Cluster<T> left, Cluster<T> right, double distance, T instance, int id)
        {
            _left = left;
            _right = right;
            Instance = instance;
            Distance = distance;
            Id = id;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsMerged => _left != null && _right != null && Instance == null;

        public bool ImmediatelyContains(Cluster<T> cluster) => cluster == _left || cluster == _right;

        public ISet<ISet<T>> GetClusteredInstances(Func<double, bool> predicate)
        {
            if (!IsMerged) return new HashSet<ISet<T>> {Instance != null ? new HashSet<T> {Instance} : new HashSet<T>()};

            var left = _left.GetClusteredInstances(predicate);
            var right = _right.GetClusteredInstances(predicate);

            if (predicate(Distance))
            {
                return new HashSet<ISet<T>>(left.Concat(right));
            }

            return new HashSet<ISet<T>>{ new HashSet<T>(left.SelectMany(s => s).Concat(right.SelectMany(s => s)))};
        }
    }
}