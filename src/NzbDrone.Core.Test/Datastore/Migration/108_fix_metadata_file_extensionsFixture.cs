using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class fix_extra_file_extensionsFixture : MigrationTest<fix_extra_file_extension>
    {
        [Test]
        public void should_extra_files_that_do_not_have_an_extension()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ExtraFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = "Series.Title.S01E01",
                    Added = "2016-05-30 20:23:02.3725923",
                    LastUpdated = "2016-05-30 20:23:02.3725923",
                    Extension = ""
                });
            });

            var items = db.Query("Select * from ExtraFiles");

            items.Should().BeEmpty();
        }

        [Test]
        public void should_fix_double_extension()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("SubtitleFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = "Series.Title.S01E01.en.srt",
                    Added = "2016-05-30 20:23:02.3725923",
                    LastUpdated = "2016-05-30 20:23:02.3725923",
                    Language = (int)Language.English,
                    Extension = "en.srt"
                });
            });

            var items = db.Query("Select * from SubtitleFiles");

            items.Should().HaveCount(1);
            items.First()["Extension"].Should().Be(".srt");
        }

        [Test]
        public void should_fix_extension_missing_a_leading_period()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ExtraFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = "Series.Title.S01E01.nfo-orig",
                    Added = "2016-05-30 20:23:02.3725923",
                    LastUpdated = "2016-05-30 20:23:02.3725923",
                    Extension = "nfo-orig"
                });
            });

            var items = db.Query("Select * from ExtraFiles");

            items.Should().HaveCount(1);
            items.First()["Extension"].Should().Be(".nfo-orig");
        }
    }
}
