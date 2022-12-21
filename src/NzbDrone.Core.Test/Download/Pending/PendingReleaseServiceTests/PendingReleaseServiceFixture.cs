using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class PendingReleaseServiceFixture : CoreTest<PendingReleaseService>
    {
        private void GivenPendingRelease()
        {
            Mocker.GetMock<IPendingReleaseRepository>()
                              .Setup(v => v.All())
                              .Returns(new List<PendingRelease>
                                {
                                      new PendingRelease { Release = new ReleaseInfo { IndexerId = 1 } }
                                });
        }

        [Test]
        public void should_not_ignore_pending_items_from_available_indexer()
        {
            Mocker.GetMock<IIndexerStatusService>()
                .Setup(v => v.GetBlockedProviders())
                .Returns(new List<IndexerStatus>());

            GivenPendingRelease();

            var results = Subject.GetPending();

            results.Should().NotBeEmpty();
            Mocker.GetMock<IMakeDownloadDecision>()
                  .Verify(v => v.GetRssDecision(It.Is<List<ReleaseInfo>>(d => d.Count == 0), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_ignore_pending_items_from_unavailable_indexer()
        {
            Mocker.GetMock<IIndexerStatusService>()
                .Setup(v => v.GetBlockedProviders())
                .Returns(new List<IndexerStatus> { new IndexerStatus { ProviderId = 1, DisabledTill = DateTime.UtcNow.AddHours(2) } });

            GivenPendingRelease();

            var results = Subject.GetPending();

            results.Should().BeEmpty();
        }
    }
}
