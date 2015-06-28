using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.PushBullet;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class pushbullet_devices_channels : MigrationTest<Core.Datastore.Migration.pushbullet_devices_channels_list>
    {
        [Test]
        public void should_convert_comma_separted_string_to_list()
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
                        ChannelTags = "channel1,channel2"
                    }.ToJson(),
                    ConfigContract = "PushBulletSettings"
                });
            });

            var items = Mocker.Resolve<NotificationRepository>().All();

            items.Should().HaveCount(1);
            var settings = items.First().Settings.As<PushBulletSettings>();
            settings.ChannelTags.Should().HaveCount(2);
        }
    }
}
