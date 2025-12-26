using System.Threading.Tasks;
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

        private async Task GivenNotification()
        {
            await Db.InsertAsync(_notification);
        }

        [Test]
        public async Task should_delete_orphaned_notificationstatus()
        {
            var status = Builder<NotificationStatus>.CreateNew()
                                                    .With(h => h.ProviderId = _notification.Id)
                                                    .BuildNew();
            await Db.InsertAsync(status);

            Subject.Clean();
            var notificationStatuses = await GetAllStoredModelsAsync();
            notificationStatuses.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_unorphaned_notificationstatus()
        {
            await GivenNotification();

            var status = Builder<NotificationStatus>.CreateNew()
                                                    .With(h => h.ProviderId = _notification.Id)
                                                    .BuildNew();
            await Db.InsertAsync(status);

            Subject.Clean();
            var notificationStatuses = await GetAllStoredModelsAsync();
            notificationStatuses.Should().HaveCount(1);
            notificationStatuses.Should().Contain(h => h.ProviderId == _notification.Id);
        }
    }
}
