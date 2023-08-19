using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class parse_language_tags_from_existing_subtitle_filesFixture : MigrationTest<parse_language_tags_from_existing_subtitle_files>
    {
        [Test]
        public void should_process_file_with_missing_null_language_tags()
        {
            var now = DateTime.UtcNow;

            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("SubtitleFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = "S01E05.eng.srt",
                    Added = now,
                    LastUpdated = now,
                    Extension = ".srt",
                    Language = 1
                });
            });

            var files = db.Query<SubtitleFile195>("SELECT * FROM \"SubtitleFiles\"").ToList();

            files.Should().HaveCount(1);
            files.First().LanguageTags.Should().HaveCount(0);
        }

        [Test]
        public void should_process_file_with_missing_language_tags()
        {
            var now = DateTime.UtcNow;

            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("SubtitleFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = "S01E05.eng.forced.srt",
                    Added = now,
                    LastUpdated = now,
                    Extension = ".srt",
                    Language = 1
                });
            });

            var files = db.Query<SubtitleFile195>("SELECT * FROM \"SubtitleFiles\"").ToList();

            files.Should().HaveCount(1);

            var languageTags = files.First().LanguageTags;

            languageTags.Should().HaveCount(1);
            languageTags.Should().Contain("forced");
        }

        [Test]
        public void should_not_process_file_with_empty_language_tags()
        {
            var now = DateTime.UtcNow;

            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("SubtitleFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = "S01E05.eng.srt",
                    Added = now,
                    LastUpdated = now,
                    Extension = ".srt",
                    Language = 1,
                    LanguageTags = "[]"
                });
            });

            var files = db.Query<SubtitleFile195>("SELECT * FROM \"SubtitleFiles\"").ToList();

            files.Should().HaveCount(1);
            files.First().LastUpdated.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(999));
            files.First().LanguageTags.Should().HaveCount(0);
        }
    }

    public class SubtitleFile195
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int? EpisodeFileId { get; set; }
        public int? SeasonNumber { get; set; }
        public string RelativePath { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Extension { get; set; }
        public int Language { get; set; }
        public List<string> LanguageTags { get; set; }
    }
}
