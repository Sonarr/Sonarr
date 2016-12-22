using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class dedupe_tagsFixture : MigrationTest<dedupe_tags>
    {
        [Test]
        public void should_not_fail_if_series_tags_are_null()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Name = "Profile1",
                    CutOff = 0,
                    Items = "[]",
                    Language = 1
                });

                c.Insert.IntoTable("Series").Row(new
                {
                    Tvdbid = 1,
                    TvRageId = 1,
                    Title = "Title1",
                    CleanTitle = "CleanTitle1",
                    Status = 1,
                    Images = "",
                    Path = "c:\\test",
                    Monitored = 1,
                    SeasonFolder = 1,
                    Runtime = 0,
                    SeriesType = 0,
                    UseSceneNumbering = 0,
                    LastInfoSync = "2000-01-01 00:00:00",
                    ProfileId = 1
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"                 
                });
            });

            var tags = db.Query<Tag69>("SELECT * FROM Tags");
            tags.Should().HaveCount(1);
        }

        [Test]
        public void should_not_fail_if_series_tags_are_empty()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Name = "Profile1",
                    CutOff = 0,
                    Items = "[]",
                    Language = 1
                });

                c.Insert.IntoTable("Series").Row(new
                {
                    Tvdbid = 1,
                    TvRageId = 1,
                    Title = "Title1",
                    CleanTitle = "CleanTitle1",
                    Status = 1,
                    Images = "",
                    Path = "c:\\test",
                    Monitored = 1,
                    SeasonFolder = 1,
                    Runtime = 0,
                    SeriesType = 0,
                    UseSceneNumbering = 0,
                    LastInfoSync = "2000-01-01 00:00:00",
                    Tags = "[]",
                    ProfileId = 1
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });
            });

            var tags = db.Query<Tag69>("SELECT * FROM Tags");
            tags.Should().HaveCount(1);
        }

        [Test]
        public void should_remove_duplicate_labels_from_tags()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });
            });

            var tags = db.Query<Tag69>("SELECT * FROM Tags");
            tags.Should().HaveCount(1);
        }

        [Test]
        public void should_not_allow_duplicate_tag_to_be_inserted()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });
            });

            Assert.That(() => db.Query("INSERT INTO Tags (Label) VALUES ('test')"), Throws.Exception);
        }

        [Test]
        public void should_replace_duplicated_tag_with_proper_tag()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Name = "Profile1",
                    CutOff = 0,
                    Items = "[]",
                    Language = 1
                });

                c.Insert.IntoTable("Series").Row(new
                {
                    Tvdbid = 1,
                    TvRageId = 1,
                    Title = "Title1",
                    CleanTitle = "CleanTitle1",
                    Status = 1,
                    Images = "",
                    Path = "c:\\test",
                    Monitored = 1,
                    SeasonFolder = 1,
                    Runtime = 0,
                    SeriesType = 0,
                    UseSceneNumbering = 0,
                    LastInfoSync = "2000-01-01 00:00:00",
                    Tags = "[2]",
                    ProfileId = 1
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });
            });

            var series = db.Query<Series69>("SELECT Tags FROM Series WHERE Id = 1").Single();
            series.Tags.First().Should().Be(1);
        }

        [Test]
        public void should_only_update_affected_series()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Name = "Profile1",
                    CutOff = 0,
                    Items = "[]",
                    Language = 1
                });

                c.Insert.IntoTable("Series").Row(new
                {
                    Tvdbid = 1,
                    TvRageId = 1,
                    Title = "Title1",
                    CleanTitle = "CleanTitle1",
                    Status = 1,
                    Images = "",
                    Path = "c:\\test",
                    Monitored = 1,
                    SeasonFolder = 1,
                    Runtime = 0,
                    SeriesType = 0,
                    UseSceneNumbering = 0,
                    LastInfoSync = "2000-01-01 00:00:00",
                    Tags = "[2]",
                    ProfileId = 1
                });

                c.Insert.IntoTable("Series").Row(new
                {
                    Tvdbid = 2,
                    TvRageId = 2,
                    Title = "Title2",
                    CleanTitle = "CleanTitle2",
                    Status = 1,
                    Images = "",
                    Path = "c:\\test",
                    Monitored = 1,
                    SeasonFolder = 1,
                    Runtime = 0,
                    SeriesType = 0,
                    UseSceneNumbering = 0,
                    LastInfoSync = "2000-01-01 00:00:00",
                    Tags = "[]",
                    ProfileId = 1
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });
            });

            var series = db.Query<Series69>("SELECT Tags FROM Series WHERE Id = 2").Single();
            series.Tags.Should().BeEmpty();
        }
    }
}
