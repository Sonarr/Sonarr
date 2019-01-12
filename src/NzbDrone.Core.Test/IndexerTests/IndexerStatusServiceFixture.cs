using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class IndexerStatusServiceFixture : CoreTest<IndexerStatusService>
    {
        private DateTime _epoch;

        [SetUp]
        public void SetUp()
        {
            _epoch = DateTime.UtcNow;

            Mocker.GetMock<IRuntimeInfo>()
                .SetupGet(v => v.StartTime)
                .Returns(_epoch - TimeSpan.FromHours(1));
        }

        private void WithStatus(IndexerStatus status)
        {
            Mocker.GetMock<IIndexerStatusRepository>()
                .Setup(v => v.FindByProviderId(1))
                .Returns(status);

            Mocker.GetMock<IIndexerStatusRepository>()
                .Setup(v => v.All())
                .Returns(new[] { status });
        }

        private void VerifyUpdate()
        {
            Mocker.GetMock<IIndexerStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<IndexerStatus>()), Times.Once());
        }

        private void VerifyNoUpdate()
        {
            Mocker.GetMock<IIndexerStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<IndexerStatus>()), Times.Never());
        }

        [Test]
        public void should_cancel_backoff_on_success()
        {
            WithStatus(new IndexerStatus { EscalationLevel = 2 });

            Subject.RecordSuccess(1);

            VerifyUpdate();

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().BeNull();
        }

        [Test]
        public void should_not_store_update_if_already_okay()
        {
            WithStatus(new IndexerStatus { EscalationLevel = 0 });

            Subject.RecordSuccess(1);

            VerifyNoUpdate();
        }
    }
}
