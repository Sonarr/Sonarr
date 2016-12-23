using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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
        }

        private void WithStatus(IndexerStatus status)
        {
            Mocker.GetMock<IIndexerStatusRepository>()
                .Setup(v => v.FindByIndexerId(1))
                .Returns(status);

            Mocker.GetMock<IIndexerStatusRepository>()
                .Setup(v => v.All())
                .Returns(new[] { status });
        }

        private void VerifyUpdate(bool updated = true)
        {
            Mocker.GetMock<IIndexerStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<IndexerStatus>()), Times.Exactly(updated ? 1 : 0));
        }

        [Test]
        public void should_start_backoff_on_first_failure()
        {
            WithStatus(new IndexerStatus());

            Subject.RecordFailure(1);

            VerifyUpdate();

            var status = Subject.GetBlockedIndexers().FirstOrDefault();
            status.Should().NotBeNull();
            status.DisabledTill.Should().HaveValue();
            status.DisabledTill.Value.Should().BeCloseTo(_epoch + TimeSpan.FromMinutes(5), 500);
        }

        [Test]
        public void should_cancel_backoff_on_success()
        {
            WithStatus(new IndexerStatus { EscalationLevel = 2 });

            Subject.RecordSuccess(1);

            VerifyUpdate();

            var status = Subject.GetBlockedIndexers().FirstOrDefault();
            status.Should().BeNull();
        }

        [Test]
        public void should_not_store_update_if_already_okay()
        {
            WithStatus(new IndexerStatus { EscalationLevel = 0 });

            Subject.RecordSuccess(1);

            VerifyUpdate(false);
        }

        [Test]
        public void should_preserve_escalation_on_intermittent_success()
        {
            WithStatus(new IndexerStatus { MostRecentFailure = _epoch - TimeSpan.FromSeconds(4), EscalationLevel = 3 });

            Subject.RecordSuccess(1);
            Subject.RecordSuccess(1);
            Subject.RecordFailure(1);

            var status = Subject.GetBlockedIndexers().FirstOrDefault();
            status.Should().NotBeNull();
            status.DisabledTill.Should().HaveValue();
            status.DisabledTill.Value.Should().BeCloseTo(_epoch + TimeSpan.FromMinutes(15), 500);
        }
    }
}
