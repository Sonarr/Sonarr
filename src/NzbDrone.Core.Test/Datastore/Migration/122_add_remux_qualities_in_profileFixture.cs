using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_remux_qualities_in_profileFixture : MigrationTest<add_remux_qualities_in_profile>
    {
        [Test]
        public void should_add_remux_to_old_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Id = 0,
                    Name = "Bluray",
                    Cutoff = 7,
                    Items = "[ { \"quality\": 7, \"allowed\": true }, { \"quality\": 19, \"allowed\": true } ]"
                });
            });

            var profiles = db.Query<Profile122>("SELECT Items FROM Profiles LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(4);
            items.Select(v => v.Quality).Should().BeEquivalentTo(7, 20, 19, 21);
            items.Select(v => v.Allowed).Should().BeEquivalentTo(true, false, true, false);
            items.Select(v => v.Name).Should().BeEquivalentTo(null, null, null, (string)null);
        }

        [Test]
        public void should_add_remux_to_old_profile_with_groups()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                                                   {
                                                       Id = 0,
                                                       Name = "Bluray",
                                                       Cutoff = 7,
                                                       Items = "[ { \"id\": 1001, \"name\": \"Why?!\", \"allowed\": true, \"items\": [{ \"quality\": 8, \"allowed\": true }, { \"quality\": 7, \"allowed\": true }] }, { \"quality\": 19, \"allowed\": true } ]"
                                                   });
            });

            var profiles = db.Query<Profile122>("SELECT Items FROM Profiles LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(4);
            items.Select(v => v.Quality).Should().BeEquivalentTo(null, 20, 19, 21);
            items.Select(v => v.Allowed).Should().BeEquivalentTo(true, false, true, false);
            items.Select(v => v.Name).Should().BeEquivalentTo("Why?!", null, null, null);
        }
    }
}
