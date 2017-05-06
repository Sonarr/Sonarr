using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.SkyhookNotifications;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.SkyhookNotifications
{
    public class SkyhookNotificationServiceFixture : CoreTest<SkyhookNotificationService>
    {
        private static readonly Version _previousVersion = new Version(BuildInfo.Version.Major, BuildInfo.Version.Minor, BuildInfo.Version.Build, BuildInfo.Version.Revision - 1);
        private static readonly Version _currentVersion = BuildInfo.Version;
        private static readonly Version _nextVersion = new Version(BuildInfo.Version.Major, BuildInfo.Version.Minor, BuildInfo.Version.Build, BuildInfo.Version.Revision + 1);

        private List<SkyhookNotification> _expiredNotifications;
        private List<SkyhookNotification> _currentNotifications;
        private List<SkyhookNotification> _futureNotifications;
        private List<SkyhookNotification> _urlNotifications;

        [SetUp]
        public void SetUp()
        {
            _expiredNotifications = new List<SkyhookNotification>
            {
                new SkyhookNotification
                {
                    Id = 1,
                    Type = SkyhookNotificationType.Notification,
                    Title = "Expired Notification",
                    MaximumVersion = _previousVersion.ToString()
                }
            };

            _currentNotifications = new List<SkyhookNotification>
            {
                new SkyhookNotification
                {
                    Id = 2,
                    Type = SkyhookNotificationType.Notification,
                    Title = "Timeless current Notification"
                },
                new SkyhookNotification
                {
                    Id = 2,
                    Type = SkyhookNotificationType.Notification,
                    Title = "Ending current Notification",
                    MaximumVersion = _currentVersion.ToString()
                },
                new SkyhookNotification
                {
                    Id = 2,
                    Type = SkyhookNotificationType.Notification,
                    Title = "Ending future Notification",
                    MaximumVersion = _nextVersion.ToString()
                },
                new SkyhookNotification
                {
                    Id = 2,
                    Type = SkyhookNotificationType.Notification,
                    Title = "Starting previous Notification",
                    MinimumVersion = _previousVersion.ToString()
                },
                new SkyhookNotification
                {
                    Id = 2,
                    Type = SkyhookNotificationType.Notification,
                    Title = "Starting current Notification",
                    MinimumVersion = _currentVersion.ToString()
                }
            };

            _futureNotifications = new List<SkyhookNotification>
            {
                new SkyhookNotification
                {
                    Id = 3,
                    Type = SkyhookNotificationType.Notification,
                    Title = "Future Notification",
                    MinimumVersion = _nextVersion.ToString()
                }
            };

            _urlNotifications = new List<SkyhookNotification>
            {
                new SkyhookNotification
                {
                    Id = 3,
                    Type = SkyhookNotificationType.UrlBlacklist,
                    Title = "Future Notification"
                },
                new SkyhookNotification
                {
                    Id = 3,
                    Type = SkyhookNotificationType.UrlReplace,
                    Title = "Future Notification"
                }
            };
        }

        private void GivenNotifications(List<SkyhookNotification> notifications)
        {
            Mocker.GetMock<ISkyhookNotificationProxy>()
                  .Setup(v => v.GetNotifications())
                  .Returns(notifications);
        }

        [Test]
        public void should_not_return_expired_notifications()
        {
            GivenNotifications(_expiredNotifications);

            Subject.GetUserNotifications().Should().BeEmpty();
        }

        [Test]
        public void should_not_return_future_notifications()
        {
            GivenNotifications(_futureNotifications);

            Subject.GetUserNotifications().Should().BeEmpty();
        }

        [Test]
        public void should_return_current_notifications()
        {
            GivenNotifications(_currentNotifications);

            Subject.GetUserNotifications().Should().HaveCount(_currentNotifications.Count);
        }

        [Test]
        public void should_not_return_user_notifications()
        {
            GivenNotifications(_currentNotifications);

            Subject.GetUrlNotifications().Should().BeEmpty();
        }

        [Test]
        public void should_not_return_url_notifications()
        {
            GivenNotifications(_urlNotifications);

            Subject.GetUserNotifications().Should().BeEmpty();
        }

        [Test]
        public void should_return_url_notifications()
        {
            GivenNotifications(_urlNotifications);

            Subject.GetUrlNotifications().Should().HaveCount(_urlNotifications.Count);
        }

        [Test]
        public void should_cache_api_result()
        {
            GivenNotifications(_urlNotifications);

            Subject.GetUrlNotifications();
            Subject.GetUrlNotifications();

            Mocker.GetMock<ISkyhookNotificationProxy>()
                  .Verify(v => v.GetNotifications(), Times.Once());
        }
    }
}
