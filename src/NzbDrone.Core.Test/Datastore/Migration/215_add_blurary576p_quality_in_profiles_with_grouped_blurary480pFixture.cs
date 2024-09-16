using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_blurary576p_quality_in_profiles_with_grouped_blurary480pFixture : MigrationTest<add_blurary576p_quality_in_profiles_with_grouped_blurary480p>
    {
        private string GenerateQualityJson(int quality, bool allowed)
        {
            return $"{{ \"quality\": {quality}, \"allowed\": {allowed.ToString().ToLowerInvariant()} }}";
        }

        private string GenerateQualityGroupJson(int id, string name, int[] qualities, bool allowed)
        {
            return $"{{ \"id\": {id}, \"name\": \"{name}\", \"items\": [{string.Join(", ", qualities.Select(q => $"{{ \"quality\": {q} }}"))}], \"allowed\": {allowed.ToString().ToLowerInvariant()} }}";
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

        [Test]
        public void should_not_allow_bluray576p_if_blurary480p_not_allowed()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Id = 0,
                    Name = "Bluray",
                    Cutoff = 7,
                    Items = $"[{GenerateQualityJson((int)Quality.DVD, true)}, {GenerateQualityJson((int)Quality.Bluray480p, false)}, {GenerateQualityJson((int)Quality.Bluray720p, false)}]"
                });
            });

            var profiles = db.Query<Profile122>("SELECT \"Items\" FROM \"QualityProfiles\" LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(4);
            items.Select(v => v.Quality).Should().Equal((int)Quality.DVD, (int)Quality.Bluray480p, (int)Quality.Bluray576p, (int)Quality.Bluray720p);
            items.Select(v => v.Allowed).Should().Equal(true, false, false, false);
            items.Select(v => v.Name).Should().Equal(null, null, null, null);
        }

        [Test]
        public void should_add_bluray576p_to_old_profile_with_grouped_bluray_480p()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Id = 0,
                    Name = "Bluray",
                    Cutoff = 7,
                    Items = $"[{GenerateQualityGroupJson(1000, "DVD", new[] { (int)Quality.DVD, (int)Quality.Bluray480p }, true)}, {GenerateQualityJson((int)Quality.Bluray720p, false)}]"
                });
            });

            var profiles = db.Query<Profile122>("SELECT \"Items\" FROM \"QualityProfiles\" LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(3);
            items.Select(v => v.Quality).Should().Equal(null, (int)Quality.Bluray576p, (int)Quality.Bluray720p);
            items.Select(v => v.Id).Should().Equal(1000, 0, 0);
            items.Select(v => v.Allowed).Should().Equal(true, true, false);
            items.Select(v => v.Name).Should().Equal("DVD", null, null);
        }

        [Test]
        public void should_not_add_bluray576p_to_profile_with_bluray_576p()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Id = 0,
                    Name = "Bluray",
                    Cutoff = 7,
                    Items = $"[{GenerateQualityJson((int)Quality.DVD, true)}, {GenerateQualityJson((int)Quality.Bluray480p, false)}, {GenerateQualityJson((int)Quality.Bluray576p, false)}, {GenerateQualityJson((int)Quality.Bluray720p, false)}]"
                });
            });

            var profiles = db.Query<Profile122>("SELECT \"Items\" FROM \"QualityProfiles\" LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(4);
            items.Select(v => v.Quality).Should().Equal((int)Quality.DVD, (int)Quality.Bluray480p, (int)Quality.Bluray576p, (int)Quality.Bluray720p);
            items.Select(v => v.Allowed).Should().Equal(true, false, false, false);
            items.Select(v => v.Name).Should().Equal(null, null, null, null);
        }

        [Test]
        public void should_not_add_bluray576p_to_profile_with_grouped_bluray_576p()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Id = 0,
                    Name = "Bluray",
                    Cutoff = 7,
                    Items = $"[{GenerateQualityGroupJson(1000, "DVD", new[] { (int)Quality.DVD, (int)Quality.Bluray480p, (int)Quality.Bluray576p }, true)}, {GenerateQualityJson((int)Quality.Bluray720p, false)}]"
                });
            });

            var profiles = db.Query<Profile122>("SELECT \"Items\" FROM \"QualityProfiles\" LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(2);
            items.Select(v => v.Quality).Should().Equal(null, (int)Quality.Bluray720p);
            items.Select(v => v.Id).Should().Equal(1000, 0);
            items.Select(v => v.Allowed).Should().Equal(true, false);
            items.Select(v => v.Name).Should().Equal("DVD", null);
            items.First().Items.Select(v => v.Quality).Should().Equal((int)Quality.DVD, (int)Quality.Bluray480p, (int)Quality.Bluray576p);
        }
    }
}
