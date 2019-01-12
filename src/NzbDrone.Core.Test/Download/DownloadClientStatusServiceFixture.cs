using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Download;
using NzbDrone.Core.Test.Framework;

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
