using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Join;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedNotificationStatusFixture : DbTest<CleanupOrphanedNotificationStatus, NotificationStatus>
    {
        private NotificationDefinition _notification;

        [SetUp]
        public void Setup()
        {
            _notification = Builder<NotificationDefinition>.CreateNew()
                                                           .With(s => s.Settings = new JoinSettings { })
                                                           .BuildNew();
        }

        private void GivenNotification()
        {
            Db.Insert(_notification);
        }

        [Test]
        public void should_delete_orphaned_notificationstatus()
        {
            var status = Builder<NotificationStatus>.CreateNew()
                                                    .With(h => h.ProviderId = _notification.Id)
                                                    .BuildNew();
            Db.Insert(status);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_notificationstatus()
        {
            GivenNotification();

            var status = Builder<NotificationStatus>.CreateNew()
                                                    .With(h => h.ProviderId = _notification.Id)
                                                    .BuildNew();
            Db.Insert(status);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.ProviderId == _notification.Id);
        }
    }
}
