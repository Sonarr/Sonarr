using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_ultrahd_quality_in_profilesFixture : MigrationTest<add_ultrahd_quality_in_profiles>
    {
        [Test]
        public void should_add_ultrahd_to_old_profile()
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
            items.Should().HaveCount(4);
            items.Select(v => v.Quality).Should().BeEquivalentTo(1, 16, 18, 19);
            items.Select(v => v.Allowed).Should().BeEquivalentTo(true, false, false, false);
        }
    }
}
