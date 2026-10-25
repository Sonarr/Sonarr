using System.Data;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration;

[TestFixture]
public class series_title_slug_not_uniqueFixture : MigrationTest<series_title_slug_not_unique>
{
    [Test]
    public void should_allow_series_to_share_a_title_slug()
    {
        var db = WithDapperMigrationTestDb();

        InsertSeries(db, tvdbId: 1, titleSlug: "breaking-bad");
        InsertSeries(db, tvdbId: 2, titleSlug: "breaking-bad");

        db.QuerySingle<int>("SELECT COUNT(*) FROM \"Series\" WHERE \"TitleSlug\" = 'breaking-bad'").Should().Be(2);
    }

    private static void InsertSeries(IDbConnection db, int tvdbId, string titleSlug)
    {
        db.Execute(
            @"INSERT INTO ""Series"" (""TvdbId"", ""TmdbId"", ""Title"", ""CleanTitle"", ""TitleSlug"", ""Status"", ""Images"", ""Path"", ""Monitored"",
                                      ""SeasonFolder"", ""Runtime"", ""SeriesType"", ""UseSceneNumbering"", ""TvRageId"", ""TvMazeId"",
                                      ""OriginalLanguage"", ""MonitorNewItems"", ""MalIds"", ""AniListIds"")
              VALUES (@TvdbId, 0, 'Title', 'title', @TitleSlug, 1, '[]', @Path, @Monitored, @SeasonFolder, 0, 0, @UseSceneNumbering, 0, 0, 1, 0, '[]', '[]')",
            new { TvdbId = tvdbId, TitleSlug = titleSlug, Path = $"/series/{tvdbId}", Monitored = true, SeasonFolder = true, UseSceneNumbering = false });
    }
}
