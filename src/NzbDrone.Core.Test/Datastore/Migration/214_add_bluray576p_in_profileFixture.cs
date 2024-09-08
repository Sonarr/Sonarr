using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_bluray576p_in_profileFixture : MigrationTest<add_blurary576p_quality_in_profiles>
    {
        private string GenerateQualityJson(int quality, bool allowed)
        {
            return $"{{ \"quality\": {quality}, \"allowed\": {allowed.ToString().ToLowerInvariant()} }}";
        }

        [Test]
        public void should_add_bluray576p_to_old_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Id = 0,
                    Name = "Bluray",
                    Cutoff = 7,
                    Items = $"[{GenerateQualityJson((int)Quality.DVD, true)}, {GenerateQualityJson((int)Quality.Bluray480p, true)}, {GenerateQualityJson((int)Quality.Bluray720p, false)}]"
                });
            });

            var profiles = db.Query<Profile122>("SELECT \"Items\" FROM \"QualityProfiles\" LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(4);
            items.Select(v => v.Quality).Should().Equal((int)Quality.DVD, (int)Quality.Bluray480p, (int)Quality.Bluray576p, (int)Quality.Bluray720p);
            items.Select(v => v.Allowed).Should().Equal(true, true, true, false);
            items.Select(v => v.Name).Should().Equal(null, null, null, null);
        }
    }
}
