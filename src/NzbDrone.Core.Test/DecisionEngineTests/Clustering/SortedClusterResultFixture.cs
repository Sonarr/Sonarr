using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Clustering;

namespace NzbDrone.Core.Test.DecisionEngineTests.Clustering
{
    [TestFixture]
    public class SortedClusterResultFixture
    {
        [TestCaseSource(nameof(SortedClusterResultTestCases))]
        public void ClusterTests(ISet<ISet<TestClusterCandidate>> clusteredInstances, IList<TestClusterCandidate> expectedResults)
        {
            var subject = new SortedClusterResult<TestClusterCandidate>(clusteredInstances, TestClusterCandidate.SortingKeyFunction);
            subject.SelectMany(c => c.OrderByDescending(t => t)).Should().BeEquivalentTo(expectedResults);
        }
        
        private static readonly object[] SortedClusterResultTestCases =
        {
            new object[]
            {
                TestClusterCandidate.ToClusterSets(new[] {500, 603}, new[] {4, 5}),
                TestClusterCandidate.ToTestList(5, 4, 603, 500)
            },

            new object[]
            {
                TestClusterCandidate.ToClusterSets(new[] {499, 501}, new[] {20, 10}, new[] {700, 710}),
                TestClusterCandidate.ToTestList(20, 10, 501, 499, 710, 700)
            }
        };
    }
}
