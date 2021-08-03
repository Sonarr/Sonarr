using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class pushbullet_device_idsFixture : MigrationTest<pushbullet_device_ids>
    {
        [Test]
        public void should_not_fail_if_no_pushbullet()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Notifications").Row(new
                {
                    OnGrab = false,
                    OnDownload = false,
                    OnUpgrade = false,
                    Name = "Pushover",
                    Implementation = "Pushover",
                    Settings = "{}",
                    ConfigContract = "PushoverSettings"
                });
            });

            var items = db.Query<Notification86>("SELECT * FROM Notifications");

            items.Should().HaveCount(1);
        }

        [Test]
        public void should_not_fail_if_deviceId_is_not_set()
        {
            var db = WithMigrationTestDb(c =>
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
                        ApiKey = "my_api_key"
                    }.ToJson(),
                    ConfigContract = "PushBulletSettings"
                });
            });

            var items = db.Query<Notification86>("SELECT * FROM Notifications");

            items.Should().HaveCount(1);
        }

        [Test]
        public void should_add_deviceIds_setting_matching_deviceId()
        {
            var deviceId = "device_id";

            var db = WithMigrationTestDb(c =>
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

            var items = db.Query<Notification86>("SELECT * FROM Notifications");

            items.Should().HaveCount(1);
            items.First().Settings.ToObject<PushBulletSettings86>().DeviceIds.First().Should().Be(deviceId);
        }
    }
}
