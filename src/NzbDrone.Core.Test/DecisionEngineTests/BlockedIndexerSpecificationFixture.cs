using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using Workarr.DecisionEngine;
using Workarr.DecisionEngine.Specifications;
using Workarr.Indexers;
using Workarr.Parser.Model;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class BlockedIndexerSpecificationFixture : CoreTest<BlockedIndexerSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode
            {
                Release = new ReleaseInfo { IndexerId = 1 }
            };

            Mocker.GetMock<IIndexerStatusService>()
                  .Setup(v => v.GetBlockedProviders())
                  .Returns(new List<IndexerStatus>());
        }

        private void WithBlockedIndexer()
        {
            Mocker.GetMock<IIndexerStatusService>()
                  .Setup(v => v.GetBlockedProviders())
                  .Returns(new List<IndexerStatus> { new IndexerStatus { ProviderId = 1, DisabledTill = DateTime.UtcNow } });
        }

        [Test]
        public void should_return_true_if_no_blocked_indexer()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_blocked_indexer()
        {
            WithBlockedIndexer();

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
            Subject.Type.Should().Be(RejectionType.Temporary);
        }
    }
}
