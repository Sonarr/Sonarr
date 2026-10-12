using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Download;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.Test.Download
{
    public class DownloadClientStatusServiceFixture : CoreTest<DownloadClientStatusService>
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

        private DownloadClientStatus WithStatus(DownloadClientStatus status)
        {
            Mocker.GetMock<IDownloadClientStatusRepository>()
                .Setup(v => v.FindByProviderId(1))
                .Returns(status);

            Mocker.GetMock<IDownloadClientStatusRepository>()
                .Setup(v => v.All())
                .Returns(new[] { status });

            return status;
        }

        private void VerifyUpdate()
        {
            Mocker.GetMock<IDownloadClientStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<DownloadClientStatus>()), Times.Once());
        }

        private void VerifyNoUpdate()
        {
            Mocker.GetMock<IDownloadClientStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<DownloadClientStatus>()), Times.Never());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void should_record_submission_failure_and_cap_count_at_three(int failures)
        {
            var status = WithStatus(new DownloadClientStatus { ProviderId = 1, SubmissionFailureCount = failures });

            Subject.RecordSubmissionFailure(1);

            status.SubmissionFailureCount.Should().Be(Math.Min(3, failures + 1));
            status.MostRecentSubmissionFailure.Should().BeOnOrAfter(_epoch);
            Subject.GetBlockedProviders().Should().Contain(status);
            VerifyUpdate();
            VerifyEventPublished<ProviderStatusChangedEvent<IDownloadClient>>();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void should_only_remain_paused_after_three_submission_failures(int failures)
        {
            var status = WithStatus(new DownloadClientStatus
            {
                ProviderId = 1,
                SubmissionFailureCount = failures,
                MostRecentSubmissionFailure = _epoch.AddDays(-1)
            });

            status.IsDisabled().Should().Be(failures == 3);
        }

        [Test]
        public void queue_success_should_not_clear_submission_pause()
        {
            var status = WithStatus(new DownloadClientStatus
            {
                ProviderId = 1,
                SubmissionFailureCount = 3,
                MostRecentSubmissionFailure = _epoch.AddDays(-1),
                EscalationLevel = 2,
                DisabledTill = _epoch.AddMinutes(5)
            });

            Subject.RecordSuccess(1);

            status.DisabledTill.Should().BeNull();
            status.SubmissionFailureCount.Should().Be(3);
            Subject.GetBlockedProviders().Should().Contain(status);
        }

        [TestCase(1)]
        [TestCase(3)]
        public void successful_submission_should_clear_count_and_pause(int failures)
        {
            var status = WithStatus(new DownloadClientStatus
            {
                ProviderId = 1,
                SubmissionFailureCount = failures,
                MostRecentSubmissionFailure = _epoch
            });

            Subject.RecordSubmissionSuccess(1);

            status.SubmissionFailureCount.Should().Be(0);
            status.MostRecentSubmissionFailure.Should().BeNull();
            Subject.GetBlockedProviders().Should().BeEmpty();
            VerifyUpdate();
            VerifyEventPublished<ProviderStatusChangedEvent<IDownloadClient>>();
        }

        [Test]
        public void queue_failure_should_not_count_as_submission_failure()
        {
            var status = WithStatus(new DownloadClientStatus { ProviderId = 1 });

            Subject.RecordFailure(1);

            status.SubmissionFailureCount.Should().Be(0);
        }

        [Test]
        public void should_not_consider_blocked_within_5_minutes_since_initial_failure()
        {
            WithStatus(new DownloadClientStatus
            {
                InitialFailure = _epoch - TimeSpan.FromMinutes(4),
                MostRecentFailure = _epoch - TimeSpan.FromSeconds(4),
                EscalationLevel = 3
            });

            Subject.RecordFailure(1);

            VerifyUpdate();

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().BeNull();
        }

        [Test]
        public void should_consider_blocked_after_5_minutes_since_initial_failure()
        {
            WithStatus(new DownloadClientStatus
            {
                InitialFailure = _epoch - TimeSpan.FromMinutes(6),
                MostRecentFailure = _epoch - TimeSpan.FromSeconds(120),
                EscalationLevel = 3
            });

            Subject.RecordFailure(1);

            VerifyUpdate();

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().NotBeNull();
        }

        [Test]
        public void should_not_escalate_further_till_after_5_minutes_since_initial_failure()
        {
            var origStatus = WithStatus(new DownloadClientStatus
            {
                InitialFailure = _epoch - TimeSpan.FromMinutes(4),
                MostRecentFailure = _epoch - TimeSpan.FromSeconds(4),
                EscalationLevel = 3
            });

            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().BeNull();

            origStatus.EscalationLevel.Should().Be(3);
        }

        [Test]
        public void should_escalate_further_after_5_minutes_since_initial_failure()
        {
            WithStatus(new DownloadClientStatus
            {
                InitialFailure = _epoch - TimeSpan.FromMinutes(6),
                MostRecentFailure = _epoch - TimeSpan.FromSeconds(120),
                EscalationLevel = 3
            });

            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().NotBeNull();

            status.EscalationLevel.Should().BeGreaterThan(3);
        }

        [Test]
        public void should_not_escalate_beyond_3_hours()
        {
            WithStatus(new DownloadClientStatus
            {
                InitialFailure = _epoch - TimeSpan.FromMinutes(6),
                MostRecentFailure = _epoch - TimeSpan.FromSeconds(120),
                EscalationLevel = 3
            });

            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().NotBeNull();
            status.DisabledTill.Should().HaveValue();
            status.DisabledTill.Should().NotBeAfter(_epoch + TimeSpan.FromHours(3.1));
        }
    }
}
