using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Test.ProviderTests.NotificationProviderTests
{
    [TestFixture]
    public class NotificationProviderFixture
    {
        NotificationProvider _notificationProvider;

        [SetUp]
        public void Setup()
        {
            _notificationProvider = new NotificationProvider();
        }

        [Test]
        public void current_notification_should_return_null_at_start()
        {
            _notificationProvider.GetCurrent().Should().BeNull();
        }

        [Test]
        public void should_return_current_on_active_notifications()
        {
            var fakeNotification = new ProgressNotification("Title");
            _notificationProvider.Register(fakeNotification);

            _notificationProvider.GetCurrent().Should().Be(fakeNotification);
        }

        [Test]
        public void should_return_last_if_recently_completed()
        {
            var fakeNotification = new ProgressNotification("Title");
            _notificationProvider.Register(fakeNotification);
            fakeNotification.Dispose();

            _notificationProvider.GetCurrent().Should().Be(fakeNotification);
        }

        [Test]
        public void should_return_null_if_completed_long_time_ago()
        {
            var fakeNotification = new ProgressNotification("Title");
            _notificationProvider.Register(fakeNotification);
            fakeNotification.Dispose();

            Thread.Sleep(7000);

            _notificationProvider.GetCurrent().Should().BeNull();
        }

        [Test]
        public void new_notification_should_replace_old_one()
        {
            var oldNotification = new ProgressNotification("Title");
            _notificationProvider.Register(oldNotification);

            var newNotification = new ProgressNotification("Title");
            _notificationProvider.Register(newNotification);

            _notificationProvider.GetCurrent().Should().Be(newNotification);
        }

    }
}
