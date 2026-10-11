using System;
using System.Linq;
using Dapper;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class pending_releases_season_numbersFixture : MigrationTest<pending_releases_season_numbers>
    {
        private JObject MigratePendingRelease(string parsedEpisodeInfo)
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("PendingReleases").Row(new
                {
                    SeriesId = 1,
                    Title = "Series Title",
                    Added = DateTime.UtcNow,
                    ParsedEpisodeInfo = parsedEpisodeInfo,
                    Release = "{}",
                    Reason = (int)PendingReleaseReason.Delay
                });
            });

            return Json.Deserialize<JObject>(db.Query<string>("SELECT \"ParsedEpisodeInfo\" FROM \"PendingReleases\"").First());
        }

        [Test]
        public void should_move_season_number_to_season_numbers()
        {
            var result = MigratePendingRelease("{\"seriesTitle\":\"Series Title\",\"seasonNumber\":2,\"episodeNumbers\":[10]}");

            result.ContainsKey("seasonNumber").Should().BeFalse();
            result["seasonNumbers"].Values<int>().Should().Equal(2);
            result["episodeNumbers"].Values<int>().Should().Equal(10);
        }

        [Test]
        public void should_set_empty_season_numbers_when_season_number_is_null()
        {
            var result = MigratePendingRelease("{\"seriesTitle\":\"Series Title\",\"seasonNumber\":null,\"airDate\":\"2026-01-01\"}");

            result.ContainsKey("seasonNumber").Should().BeFalse();
            result["seasonNumbers"].Values<int>().Should().BeEmpty();
        }

        [Test]
        public void should_keep_existing_season_numbers()
        {
            var result = MigratePendingRelease("{\"seriesTitle\":\"Series Title\",\"seasonNumber\":1,\"seasonNumbers\":[1,2,3],\"fullSeason\":true}");

            result.ContainsKey("seasonNumber").Should().BeFalse();
            result["seasonNumbers"].Values<int>().Should().Equal(1, 2, 3);
        }

        [Test]
        public void should_not_change_rows_without_season_number()
        {
            var result = MigratePendingRelease("{\"seriesTitle\":\"Series Title\",\"seasonNumbers\":[4],\"episodeNumbers\":[2]}");

            result["seasonNumbers"].Values<int>().Should().Equal(4);
            result["episodeNumbers"].Values<int>().Should().Equal(2);
        }
    }
}
