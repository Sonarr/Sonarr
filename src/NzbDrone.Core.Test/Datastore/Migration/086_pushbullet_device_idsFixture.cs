using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.PushBullet;
using NzbDrone.Core.Notifications.Pushover;
using NzbDrone.Core.Test.Framework;
namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class pushbullet_device_idsFixture : MigrationTest<Core.Datastore.Migration.pushbullet_device_ids>
    {
        [Test]
        public void should_not_fail_if_no_pushbullet()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new 
                {
                    OnGrab = false,
                    OnDownload = false,
                    OnUpgrade = false,
                    Name = "Pushover",
                    Implementation = "Pushover",
                    Settings = new PushoverSettings().ToJson(),
                    ConfigContract = "PushoverSettings"
                });
            });

            var items = Mocker.Resolve<NotificationRepository>().All();

            items.Should().HaveCount(1);
        }

        [Test]
        public void should_not_fail_if_deviceId_is_not_set()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new
                {
                    OnGrab = false,
                    OnDownload = false,
                    OnUpgrade = false,
                    Name = "PushBullet",
                    Implementation = "PushBullet",
                    Settings = new
                    {
                        ApiKey = "my_api_key",
                    }.ToJson(),
                    ConfigContract = "PushBulletSettings"
                });
            });

            var items = Mocker.Resolve<NotificationRepository>().All();

            items.Should().HaveCount(1);
        }

        [Test]
        public void should_add_deviceIds_setting_matching_deviceId()
        {
            var deviceId = "device_id";

            WithTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new
                {
                    OnGrab = false,
                    OnDownload = false,
                    OnUpgrade = false,
                    Name = "PushBullet",
                    Implementation = "PushBullet",
                    Settings = new 
                    {
                        ApiKey = "my_api_key",
                        DeviceId = deviceId
                    }.ToJson(),
                    ConfigContract = "PushBulletSettings"
                });
            });

            var items = Mocker.Resolve<NotificationRepository>().All();

            items.Should().HaveCount(1);
            items.First().Settings.As<PushBulletSettings>().DeviceIds.First().Should().Be(deviceId);
        }
    }
}
