using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_webrip_and_br480_qualites_in_profileFixture : MigrationTest<add_webrip_and_br480_qualites_in_profile>
    {
        private string GenerateQualityJson(int quality, bool allowed)
        {
            return $"{{ \"quality\": {quality}, \"allowed\": {allowed.ToString().ToLowerInvariant()} }}";
        }

        [Test]
        public void should_add_webrip_qualities_and_group_with_webdl()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Id = 0,
                    Name = "SDTV",
                    Cutoff = 1,
                    Items = $"[{GenerateQualityJson(1, true)}, {GenerateQualityJson((int)Quality.WEBRip480p, false)}, {GenerateQualityJson((int)Quality.WEBRip720p, false)}, {GenerateQualityJson((int)Quality.WEBRip1080p, false)}, {GenerateQualityJson((int)Quality.WEBRip2160p, false)}]"
                });
            });

            var profiles = db.Query<Profile117>("SELECT \"Items\" FROM \"Profiles\" LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(6);
            items.Select(v => v.Quality).Should().Equal(1, null, null, null, null, null);
            items.Select(v => v.Items.Count).Should().Equal(0, 2, 2, 2, 2, 2);
            items.Select(v => v.Allowed).Should().Equal(true, false, false, false, false, false);
        }

        [Test]
        public void should_add_bluray480p_quality_and_group_with_dvd()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Id = 0,
                    Name = "SDTV",
                    Cutoff = 1,
                    Items = $"[{GenerateQualityJson(1, true)}, {GenerateQualityJson((int)Quality.DVD, false)}, {GenerateQualityJson((int)Quality.Bluray480p, false)}]"
                });
            });

            var profiles = db.Query<Profile117>("SELECT \"Items\" FROM \"Profiles\" LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(6);
            items.Select(v => v.Quality).Should().Equal(1, null, null, null, null, null);
            items.Select(v => v.Items.Count).Should().Equal(0, 2, 2, 2, 2, 2);
            items.Select(v => v.Allowed).Should().Equal(true, false, false, false, false, false);
        }

        [Test]
        public void should_add_webrip_and_webdl_if_webdl_is_missing()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Id = 0,
                    Name = "SDTV",
                    Cutoff = 1,
                    Items = $"[{GenerateQualityJson(1, true)}, {GenerateQualityJson((int)Quality.WEBRip480p, false)}, {GenerateQualityJson((int)Quality.WEBRip720p, false)}, {GenerateQualityJson((int)Quality.WEBRip1080p, false)}]"
                });
            });

            var profiles = db.Query<Profile117>("SELECT \"Items\" FROM \"Profiles\" LIMIT 1");

            var items = profiles.First().Items;
            items.Should().HaveCount(6);
            items.Select(v => v.Quality).Should().Equal(1, null, null, null, null, null);
            items.Select(v => v.Items.Count).Should().Equal(0, 2, 2, 2, 2, 2);
            items.Select(v => v.Allowed).Should().Equal(true, false, false, false, false, false);
        }

        [Test]
        public void should_group_webrip_and_webdl_with_the_same_resolution()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Id = 0,
                    Name = "SDTV",
                    Cutoff = 1,
                    Items = $"[{GenerateQualityJson(1, true)}, {GenerateQualityJson((int)Quality.WEBRip480p, false)}, {GenerateQualityJson((int)Quality.WEBRip720p, false)}, {GenerateQualityJson((int)Quality.WEBRip1080p, false)}, {GenerateQualityJson((int)Quality.WEBRip2160p, false)}]"
                });
            });

            var profiles = db.Query<Profile117>("SELECT \"Items\" FROM \"Profiles\" LIMIT 1");
            var items = profiles.First().Items;

            items[1].Items.First().Quality.Should().Be((int)Quality.WEBRip480p);
            items[1].Items.Last().Quality.Should().Be((int)Quality.WEBDL480p);

            items[2].Items.First().Quality.Should().Be((int)Quality.DVD);
            items[2].Items.Last().Quality.Should().Be((int)Quality.Bluray480p);

            items[3].Items.First().Quality.Should().Be((int)Quality.WEBRip720p);
            items[3].Items.Last().Quality.Should().Be((int)Quality.WEBDL720p);

            items[4].Items.First().Quality.Should().Be((int)Quality.WEBRip1080p);
            items[4].Items.Last().Quality.Should().Be((int)Quality.WEBDL1080p);

            items[5].Items.First().Quality.Should().Be((int)Quality.WEBRip2160p);
            items[5].Items.Last().Quality.Should().Be((int)Quality.WEBDL2160p);
        }
    }
}
