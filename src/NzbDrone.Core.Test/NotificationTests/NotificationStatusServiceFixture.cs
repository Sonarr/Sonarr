using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests
{
    public class NotificationStatusServiceFixture : CoreTest<NotificationStatusService>
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

        private NotificationStatus WithStatus(NotificationStatus status)
        {
            Mocker.GetMock<INotificationStatusRepository>()
                .Setup(v => v.FindByProviderId(1))
                .Returns(status);

            Mocker.GetMock<INotificationStatusRepository>()
                .Setup(v => v.All())
                .Returns(new[] { status });

            return status;
        }

        private void VerifyUpdate()
        {
            Mocker.GetMock<INotificationStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<NotificationStatus>()), Times.Once());
        }

        private void VerifyNoUpdate()
        {
            Mocker.GetMock<INotificationStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<NotificationStatus>()), Times.Never());
        }

        [Test]
        public void should_not_consider_blocked_within_5_minutes_since_initial_failure()
        {
            WithStatus(new NotificationStatus
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
            WithStatus(new NotificationStatus
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
            var origStatus = WithStatus(new NotificationStatus
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
            WithStatus(new NotificationStatus
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
            WithStatus(new NotificationStatus
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
