using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class dedupe_tagsFixture : MigrationTest<Core.Datastore.Migration.dedupe_tags>
    {
        [Test]
        public void should_not_fail_if_series_tags_are_null()
        {
            WithTestDb(c =>
            {
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
                    LastInfoSync = "2000-01-01 00:00:00"
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"                 
                });
            });

            Mocker.Resolve<TagRepository>().All().Should().HaveCount(1);
        }

        [Test]
        public void should_not_fail_if_series_tags_are_empty()
        {
            WithTestDb(c =>
            {
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
                    Tags = "[]"
                });

                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });
            });

            Mocker.Resolve<TagRepository>().All().Should().HaveCount(1);
        }

        [Test]
        public void should_remove_duplicate_labels_from_tags()
        {
            WithTestDb(c =>
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

            Mocker.Resolve<TagRepository>().All().Should().HaveCount(1);
        }

        [Test]
        public void should_not_allow_duplicate_tag_to_be_inserted()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("Tags").Row(new
                {
                    Label = "test"
                });
            });

            Assert.That(() => Mocker.Resolve<TagRepository>().Insert(new Tag { Label = "test" }), Throws.Exception);
        }

        [Test]
        public void should_replace_duplicated_tag_with_proper_tag()
        {
            WithTestDb(c =>
            {
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
                    Tags = "[2]"
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

            Mocker.Resolve<SeriesRepository>().Get(1).Tags.First().Should().Be(1);
        }

        [Test]
        public void should_only_update_affected_series()
        {
            WithTestDb(c =>
            {
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
                    Tags = "[2]"
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
                    Tags = "[]"
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

            Mocker.Resolve<SeriesRepository>().Get(2).Tags.Should().BeEmpty();
        }
    }
}
