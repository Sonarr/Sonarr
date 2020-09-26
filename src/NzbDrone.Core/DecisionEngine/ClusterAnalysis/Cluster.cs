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
        public readonly bool IsMerged;
        public readonly bool HasInstance;

        public static Cluster<T> Empty() => new Cluster<T>();

        public IEnumerable<T> Contents
        {
            get
            {
                if (_left == null && _right == null && !HasInstance) return new List<T>();
                if (_left == null && _right == null) return new List<T> { Instance };

                var leftContents = _left?.Contents ?? new List<T>();
                var rightContents = _right?.Contents ?? new List<T>();

                return leftContents.Concat(rightContents);
            }
        }

        private Cluster() : this(null, null, 0, default(T), 0, false, false)
        {

        }

        public Cluster(Cluster<T> left, Cluster<T> right, double distance, int id)
            : this(left, right, distance, default(T), id, left != null && right != null, false)
        {
        }

        public Cluster(Cluster<T> left, Cluster<T> right, double distance, T instance, int id)
            : this(left, right, distance, instance, id, left != null && right != null && instance == null, instance != null)
        {
        }

        private Cluster(Cluster<T> left, Cluster<T> right, double distance, T instance, int id, bool isMerged, bool hasInstance)
        {
            _left = left;
            _right = right;
            Instance = instance;
            Distance = distance;
            Id = id;
            IsMerged = isMerged;
            HasInstance = hasInstance;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ImmediatelyContains(Cluster<T> cluster) => cluster == _left || cluster == _right;

        public ISet<ISet<T>> GetClusteredInstances(double distanceClusterPoint)
        {
            if (!IsMerged) return new HashSet<ISet<T>> {Instance != null ? new HashSet<T> {Instance} : new HashSet<T>()};

            var left = _left.GetClusteredInstances(distanceClusterPoint);
            var right = _right.GetClusteredInstances(distanceClusterPoint);

            if (Distance > distanceClusterPoint)
            {
                return new HashSet<ISet<T>>(left.Concat(right));
            }

            return new HashSet<ISet<T>>{ new HashSet<T>(left.SelectMany(s => s).Concat(right.SelectMany(s => s)))};
        }
    }
}