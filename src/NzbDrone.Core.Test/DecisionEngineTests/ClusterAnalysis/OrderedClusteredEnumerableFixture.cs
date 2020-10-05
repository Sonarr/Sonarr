using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.ClusterAnalysis;

namespace NzbDrone.Core.Test.DecisionEngineTests.ClusterAnalysis
{
    public class OrderedClusteredEnumerableFixture
    {
        [Test]
        public void should_order_by_cluster_descending_then_by_duration()
        {
            var source = new List<ClusteredEnumerableTestCandidate>
            {
                new ClusteredEnumerableTestCandidate {Size = 100, Duration = 100, Seeders = 100, Id = 1},
                new ClusteredEnumerableTestCandidate {Size = 200, Duration = 200, Seeders = 200, Id = 2},
                new ClusteredEnumerableTestCandidate {Size = 300, Duration = 300, Seeders = 300, Id = 3},
                new ClusteredEnumerableTestCandidate {Size = 1100, Duration = 1100, Seeders = 1100, Id = 4},
                new ClusteredEnumerableTestCandidate {Size = 1200, Duration = 1200, Seeders = 1200, Id = 5},
                new ClusteredEnumerableTestCandidate {Size = 1300, Duration = 1300, Seeders = 1300, Id = 6},
            };

           
            var clusters = source.Cluster();
            var ordered = clusters.OrderByClusterDescending(e => e.Size, 200)
                .ThenBy(t => t.Duration);
            var result = ordered.ToList();

            result.Select(t => t.Id).Should().Equal(4, 5, 6, 1, 2, 3);

        }

        [Test]
        public void should_order_by_duration_descending_then_by_cluster_then_by_seeders_descending()
        {
            var source = new List<ClusteredEnumerableTestCandidate>
            {
                new ClusteredEnumerableTestCandidate {Size = 100, Duration = 100, Seeders = 1, Id = 1},
                new ClusteredEnumerableTestCandidate {Size = 200, Duration = 200, Seeders = 2, Id = 2},
                new ClusteredEnumerableTestCandidate {Size = 300, Duration = 200, Seeders = 3, Id = 3},
                new ClusteredEnumerableTestCandidate {Size = 1100, Duration = 200, Seeders = 11, Id = 4},
                new ClusteredEnumerableTestCandidate {Size = 1200, Duration = 200, Seeders = 12, Id = 5},
                new ClusteredEnumerableTestCandidate {Size = 1300, Duration = 100, Seeders = 13, Id = 6},
            };

            var clusters = source.Cluster();
            var ordered = clusters.OrderByDescending(t => t.Duration)
                .ThenByCluster(v => v.Size, 200)
                .ThenByDescending(t => t.Seeders);

            var result = ordered.ToList();

            result.Select(t => t.Id).Should().Equal(3, 2, 5, 4, 1, 6);

        }


        [Test]
        public void should_order_by_multiple_clusters()
        {
            var source = new List<ClusteredEnumerableTestCandidate>
            {
                new ClusteredEnumerableTestCandidate {Size = 100, Duration = 300, Seeders = 1, Id = 1},
                new ClusteredEnumerableTestCandidate {Size = 200, Duration = 300, Seeders = 2, Id = 2},
                new ClusteredEnumerableTestCandidate {Size = 300, Duration = 200, Seeders = 3, Id = 3},
                new ClusteredEnumerableTestCandidate {Size = 1100, Duration = 200, Seeders = 11, Id = 4},
                new ClusteredEnumerableTestCandidate {Size = 1200, Duration = 100, Seeders = 12, Id = 5},
                new ClusteredEnumerableTestCandidate {Size = 1300, Duration = 100, Seeders = 13, Id = 6},
            };

            var clusters = source.Cluster();
            var ordered = clusters.OrderByClusterDescending(t => t.Duration, 100)
                .ThenByCluster(v => v.Size, 200)
                .ThenByDescending(t => t.Seeders);

            var result = ordered.ToList();

            result.Select(t => t.Id).Should().Equal(3, 2, 1, 4, 6, 5);

        }

        [Test]
        public void should_order_recursively_without_clusters()
        {
            var source = new List<int> {101, 204, 203, 105, 103};

            var expectedOrder = source
                .OrderByDescending(v => v / 100)
                .ThenBy(v => v % 100)
                .ToList();

            var orderedWithClusteredOrderedEnumerable = source
                .Cluster()
                .OrderByDescending(v => v / 100)
                .ThenBy(v => v % 100)
                .ToList();

            // 203, 204, 101, 103, 105
            orderedWithClusteredOrderedEnumerable.Should().Equal(expectedOrder);
        }


        [Test]
        public void should_order_recursively()
        {
            var source = new List<int> { 101, 204, 203, 105, 103 };

            var clusters = source.ClusterBy(v => v % 100, 2).ToList();


            var result = source
                .Cluster()
                .OrderBy(v => v / 100)
                .ThenByClusterDescending(v => v % 100, 2)
                .ToList();

            result.Should().Equal(new List<int> {105, 101, 103, 204, 203});
        }

        [Test]
        public void should_order_recursively_and_by_cluster()
        {
            var source = new List<int> { 9, 6, 5, 4, 2, 1 };

            var result = source
                .Cluster()
                .OrderBy(v => v >= 5)
                .ThenByCluster(v => v, 2)
                .ToList();

            result.Should().Equal(new List<int> { 2, 1, 4, 6, 5, 9 });
        }


    }

    internal class ClusteredEnumerableTestCandidate
    {
        public long Size;
        public long Duration;
        public long Seeders;
        public long Id;
    }
}