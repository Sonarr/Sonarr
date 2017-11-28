using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class update_animetosho_urlFixture : MigrationTest<update_animetosho_url>
    {
        [TestCase("Newznab", "https://animetosho.org")]
        [TestCase("Newznab", "http://animetosho.org")]
        [TestCase("Torznab", "https://animetosho.org")]
        [TestCase("Torznab", "http://animetosho.org")]
        public void should_replace_old_url(string impl, string baseUrl)
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Indexers").Row(new
                {
                    Name = "AnimeTosho",
                    Implementation = impl,
                    Settings = new NewznabSettings121
                    {
                        BaseUrl = baseUrl,
                        ApiPath = "/feed/nabapi"

                    }.ToJson(),
                    ConfigContract = impl + "Settings",
                    EnableInteractiveSearch = false
                });
            });

            var items = db.Query<IndexerDefinition121>("SELECT * FROM Indexers");

            items.Should().HaveCount(1);
            items.First().Settings.ToObject<NewznabSettings121>().BaseUrl.Should().Be(baseUrl.Replace("animetosho", "feed.animetosho"));
        }
    }
}
