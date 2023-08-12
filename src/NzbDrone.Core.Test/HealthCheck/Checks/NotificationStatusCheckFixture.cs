using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class NotificationStatusCheckFixture : CoreTest<NotificationStatusCheck>
    {
        private List<INotification> _notifications = new List<INotification>();
        private List<NotificationStatus> _blockedNotifications = new List<NotificationStatus>();

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<INotificationFactory>()
                  .Setup(v => v.GetAvailableProviders())
                  .Returns(_notifications);

            Mocker.GetMock<INotificationStatusService>()
                   .Setup(v => v.GetBlockedProviders())
                   .Returns(_blockedNotifications);

            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                .Returns("Some Warning Message");
        }

        private Mock<INotification> GivenNotification(int id, double backoffHours, double failureHours)
        {
            var mockNotification = new Mock<INotification>();
            mockNotification.SetupGet(s => s.Definition).Returns(new NotificationDefinition { Id = id });

            _notifications.Add(mockNotification.Object);

            if (backoffHours != 0.0)
            {
                _blockedNotifications.Add(new NotificationStatus
                {
                    ProviderId = id,
                    InitialFailure = DateTime.UtcNow.AddHours(-failureHours),
                    MostRecentFailure = DateTime.UtcNow.AddHours(-0.1),
                    EscalationLevel = 5,
                    DisabledTill = DateTime.UtcNow.AddHours(backoffHours)
                });
            }

            return mockNotification;
        }

        [Test]
        public void should_not_return_error_when_no_notifications()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_warning_if_notification_unavailable()
        {
            GivenNotification(1, 10.0, 24.0);
            GivenNotification(2, 0.0, 0.0);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_error_if_all_notifications_unavailable()
        {
            GivenNotification(1, 10.0, 24.0);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_warning_if_few_notifications_unavailable()
        {
            GivenNotification(1, 10.0, 24.0);
            GivenNotification(2, 10.0, 24.0);
            GivenNotification(3, 0.0, 0.0);

            Subject.Check().ShouldBeWarning();
        }
    }
}
