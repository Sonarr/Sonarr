using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Clustering;

namespace NzbDrone.Core.Test.DecisionEngineTests.Clustering
{
    public class InstanceInClusterGroupComparerFixture
    {
        private SortedClusterResult<TestClusterCandidate> GetClusterResult(params IEnumerable<int>[] sets) =>
            new SortedClusterResult<TestClusterCandidate>(TestClusterCandidate.ToClusterSets(sets), TestClusterCandidate.SortingKeyFunction);

        private static readonly IEnumerable<int>[] ClusterData = {new[] {700, 710}, new[] {20, 10}};

        private InstanceInClusterGroupComparer<TestClusterCandidate> _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new InstanceInClusterGroupComparer<TestClusterCandidate>(GetClusterResult(ClusterData));
        }

        [Test]
        public void NullsAreEqual()
        {
            _subject.Compare(null, null).Should().Be(0);
        }

        [Test]
        public void NullXIsLessThanY()
        {
            _subject.Compare(null, new TestClusterCandidate(10)).Should().Be(-1);
        }

        [Test]
        public void XIsGreaterThanNullY()
        {
            _subject.Compare(new TestClusterCandidate(10), null).Should().Be(1);
        }

        [Test]
        public void XNotInAnyClusterThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _subject.Compare(new TestClusterCandidate(0), new TestClusterCandidate(20)));
        }

        [Test]
        public void YNotInAnyClusterThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _subject.Compare(new TestClusterCandidate(10), new TestClusterCandidate(21)));
        }

        [Test]
        public void XAndYNotInAnyClusterThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _subject.Compare(new TestClusterCandidate(0), new TestClusterCandidate(21)));
        }

        [Test]
        public void XAndYInSameClusterShouldBeEqual()
        {
            _subject.Compare(new TestClusterCandidate(10), new TestClusterCandidate(20)).Should().Be(0);
        }

        [Test]
        public void XInBiggerClusterShouldBeGreaterThanY()
        {
            _subject.Compare(new TestClusterCandidate(700), new TestClusterCandidate(20)).Should().Be(1);
        }

        [Test]
        public void YInBiggerClusterShouldBeGreaterThanX()
        {
            _subject.Compare(new TestClusterCandidate(10), new TestClusterCandidate(710)).Should().Be(-1);
        }
    }
}
