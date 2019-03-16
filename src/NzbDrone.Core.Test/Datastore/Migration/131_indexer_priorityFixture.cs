using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class indexer_priorityFixture : MigrationTest<indexer_priority>
    {
        [Test]
        public void should_add_priority_to_settings()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Indexers").Row(new
                {
                    Id = 1,
                    Name = "Rarbg",
                    Implementation = "Rarbg",
                    Settings = "{\n  \"baseUrl\": \"https://torrentapi.org\",\n  \"rankedOnly\": false,\n  \"minimumSeeders\": 1,\n  \"seedCriteria\": {},\n}",
                    ConfigContract = "RarbgSettings",
                    EnableRss = 1,
                    EnableAutomaticSearch = 1,
                    EnableInteractiveSearch = 1
                });
            });

            var settings = db.QueryScalar<string>("SELECT Settings FROM Indexers LIMIT 1");
            var settingsJsonOjb = JObject.Parse(settings);

            settingsJsonOjb.Properties().Any(p => p.Name == "priority").Should().BeTrue();
            settingsJsonOjb.Property("priority").Value.Value<int>().ShouldBeEquivalentTo(100);
        }
    }
}
