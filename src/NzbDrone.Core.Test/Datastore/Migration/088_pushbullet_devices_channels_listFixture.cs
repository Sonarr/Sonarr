using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class pushbullet_devices_channels_listFixture : MigrationTest<pushbullet_devices_channels_list>
    {
        [Test]
        public void should_convert_comma_separted_string_to_list()
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
                        ApiKey = "my_api_key",
                        ChannelTags = "channel1,channel2"
                    }.ToJson(),
                    ConfigContract = "PushBulletSettings"
                });
            });

            var items = db.Query<Notification86>("SELECT * FROM Notifications");

            items.Should().HaveCount(1);
            items.First().Settings.ToObject<PushBulletSettings88>().ChannelTags.Should().HaveCount(2);
        }
    }
}
