using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class update_btn_url_migration_fixture : MigrationTest<update_btn_url>
    {
        [TestCase("http://api.btnapps.net")]
        [TestCase("https://api.btnapps.net")]
        [TestCase("http://api.btnapps.net/")]
        [TestCase("https://api.btnapps.net/")]
        public void should_replace_old_url(string oldUrl)
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Indexers").Row(new
                {
                    Name = "btn_old_url",
                    Implementation = "BroadcastheNet",
                    Settings = new BroadcastheNetSettings106
                    {
                        BaseUrl = oldUrl
                    }.ToJson(),
                    ConfigContract = "BroadcastheNetSettings"
                });
            });

            var items = db.Query<IndexerDefinition90>("SELECT * FROM Indexers");

            items.Should().HaveCount(1);
            items.First().Settings.ToObject<BroadcastheNetSettings106>().BaseUrl.Should().Contain("api.broadcasthe.net");
        }

        [Test]
        public void should_not_replace_other_indexers()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Indexers").Row(new
                {
                    Name = "not_btn",
                    Implementation = "NotBroadcastheNet",
                    Settings = new BroadcastheNetSettings106
                    {
                        BaseUrl = "http://api.btnapps.net",
                    }.ToJson(),
                    ConfigContract = "BroadcastheNetSettings"
                });
            });

            var items = db.Query<IndexerDefinition90>("SELECT * FROM Indexers");

            items.Should().HaveCount(1);
            items.First().Settings.ToObject<BroadcastheNetSettings106>().BaseUrl.Should().Be("http://api.btnapps.net");
        }
    }
}
