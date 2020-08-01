using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.ClusterAnalysis;

namespace NzbDrone.Core.Test.DecisionEngineTests.ClusterAnalysis
{
    [TestFixture]
    public class HierarchicalClusteringFixture
    {
        private HierarchicalClustering<TestClusterCandidate> _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new HierarchicalClustering<TestClusterCandidate>(TestClusterCandidate.DistanceFunction, Math.Max);
        }

        [Test]
        public void NullListShouldReturnEmptyCluster()
        {
            _subject.Cluster(null).Contents.Should().BeEmpty();
        }

        [Test]
        public void EmptyListShouldReturnEmptyCluster()
        {
            _subject.Cluster(new List<TestClusterCandidate>()).Contents.Should().BeEmpty();
        }

        [Test]
        public void SingleItemListShouldReturnSingleCluster()
        {
            var candidates = new List<TestClusterCandidate> {new TestClusterCandidate(1)};
            var results = _subject.Cluster(candidates);
            results.Contents.Single().Should().Be(candidates.Single());
        }

        [Test]
        public void TwoItemListShouldClusterBothItems()
        {
            var candidates = new List<TestClusterCandidate> {new TestClusterCandidate(1), new TestClusterCandidate(2)};
            var results = _subject.Cluster(candidates);
            results.Contents.Should().BeEquivalentTo(candidates);
        }

        [TestCaseSource(nameof(ClusterTestCases))]
        public void Clusters(List<TestClusterCandidate> candidates, double cutPoint,
            Func<double, double, double> linkageFunction, Func<TestClusterCandidate, TestClusterCandidate, double> distanceFunction,
            ISet<ISet<TestClusterCandidate>> expectedClusterResults)
        {
            var subject = new HierarchicalClustering<TestClusterCandidate>(distanceFunction, linkageFunction);
            var results = subject.Cluster(candidates);
            var clustered = results.GetClusteredInstances(cutPoint);
            clustered.Count.Should().Be(expectedClusterResults.Count);

            var count = expectedClusterResults
                .Sum(expectedClusterResult => clustered.Count(cluster => cluster.SetEquals(expectedClusterResult)));

            count.Should().Be(expectedClusterResults.Count);
        }

        private static readonly object[] ClusterTestCases =
        {
            new object[]
            {
                TestClusterCandidate.ToTestList(2, 4, 7, 11),
                4,
                TestClusterCandidate.CompleteLinkage,
                TestClusterCandidate.DistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {2, 4}, new[] {7, 11})
            },

            new object[]
            {
                TestClusterCandidate.ToTestList(2, 4, 7, 10),
                3,
                TestClusterCandidate.CompleteLinkage,
                TestClusterCandidate.DistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {2, 4}, new[] {7, 10})
            },

            new object[]
            {
                TestClusterCandidate.ToTestList(700, 499, 20, 501, 10, 710),
                200,
                TestClusterCandidate.SingleLinkage,
                TestClusterCandidate.DistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {10, 20}, new[] {499, 501, 700, 710})
            },

            new object[]
            {
                TestClusterCandidate.ToTestList(10, 20, 499, 501, 700, 710),
                200,
                TestClusterCandidate.CompleteLinkage,
                TestClusterCandidate.DistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {10, 20}, new[] {499, 501}, new[] {700, 710})
            },

            new object[]
            {
                TestClusterCandidate.ToTestList(1, 2, 3, 10, 99, 500, 1000, 5000, 9000, 9001, 9002, 9003, 9004),
                1,
                TestClusterCandidate.CompleteLinkage,
                TestClusterCandidate.LogDistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {1, 2, 3}, new[] {10, 99}, new[] {500, 1000},
                    new[] {5000, 9000, 9001, 9002, 9003, 9004})
            },

            new object[]
            {
                TestClusterCandidate.ToTestList(5, 500, 4, 603),
                200,
                TestClusterCandidate.CompleteLinkage,
                TestClusterCandidate.DistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {500, 603}, new[] {5, 4})
            },

            new object[]
            {
                TestClusterCandidate.ToTestList(5, 500, 4, 603, 5000),
                200,
                TestClusterCandidate.CompleteLinkage,
                TestClusterCandidate.DistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {500, 603}, new[] {5, 4}, new[] {5000})
            },

            new object[]
            {
                TestClusterCandidate.ToTestList(5, 500, 501, 430, 4, 603, 5000),
                200,
                TestClusterCandidate.CompleteLinkage,
                TestClusterCandidate.DistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {500, 603, 501, 430}, new[] {5, 4}, new[] {5000})
            },

            new object[]
            {
                TestClusterCandidate.ToTestList(1, 2, 3, 4, 5, 6, 7),
                200,
                TestClusterCandidate.CompleteLinkage,
                TestClusterCandidate.DistanceFunction,
                TestClusterCandidate.ToClusterSets(new[] {1, 2, 3, 4, 5, 6, 7})
            }
        };

    }
}
