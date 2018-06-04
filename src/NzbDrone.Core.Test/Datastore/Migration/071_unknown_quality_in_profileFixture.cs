using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class unknown_quality_in_profileFixture : MigrationTest<unknown_quality_in_profile>
    {
        [Test]
        public void should_add_unknown_to_old_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Id = 0,
                    Name = "SDTV",
                    Cutoff = 1,
                    Items = "[ { \"quality\": 1, \"allowed\": true } ]",
                    Language = 1
                });
            });

            var profiles = db.Query<Profile71>("SELECT Items FROM Profiles LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(2);
            items.First().Quality.Should().Be(0);
            items.First().Allowed.Should().Be(false);
        }
    }
}
