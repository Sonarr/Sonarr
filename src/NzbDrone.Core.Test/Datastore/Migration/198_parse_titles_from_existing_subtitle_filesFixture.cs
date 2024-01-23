using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class parse_title_from_existing_subtitle_filesFixture : MigrationTest<parse_title_from_existing_subtitle_files>
    {
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.default.eng.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].eng.default.testtitle.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].default.eng.testtitle.forced.ass", "Name (2020)/Season 1/Name (2020).mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.forced.eng.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].eng.forced.testtitle.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].forced.eng.testtitle.ass", "Name (2020)/Season 1/Name (2020).mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.default.fra.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].fra.default.testtitle.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].default.fra.testtitle.forced.ass", "Name (2020)/Season 1/Name (2020).mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.forced.fra.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].fra.forced.testtitle.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].forced.fra.testtitle.ass", "Name (2020)/Season 1/Name (2020).mkv", "testtitle", 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].default.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", null, 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].default.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", null, 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", null, 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", null, 0)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle - 3.default.eng.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 3)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle - 3.forced.eng.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 3)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].eng.forced.testtitle - 3.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 3)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].fra.default.testtitle - 3.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", "testtitle", 3)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].3.default.eng.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", null, 3)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].3.forced.eng.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", null, 3)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].eng.forced.3.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", null, 3)]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].fra.default.3.forced.ass", "Name (2020)/Season 1/Name (2020) - S01E20 - [AAC 2.0].mkv", null, 3)]
        [TestCase("Name (2020) - Name.2020.S01E03.REAL.PROPER.1080p.HEVC.x265-MeGusta - 0609901d2ea34acd81c9030980406065.en.forced.srt", "Name (2020)/Season 1/Name (2020) - Name.2020.S01E03.REAL.PROPER.1080p.HEVC.x265-MeGusta - 0609901d2ea34acd81c9030980406065.mkv", null, 0)]
        public void should_process_file_with_missing_title(string subtitlePath, string episodePath, string title, int copy)
        {
            var now = DateTime.UtcNow;

            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("SubtitleFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = subtitlePath,
                    Added = now,
                    LastUpdated = now,
                    Extension = Path.GetExtension(subtitlePath),
                    Language = 10,
                    LanguageTags = new List<string> { "sdh" }.ToJson()
                });

                c.Insert.IntoTable("EpisodeFiles").Row(new
                {
                    Id = 1,
                    SeriesId = 1,
                    RelativePath = episodePath,
                    Quality = new { }.ToJson(),
                    Size = 0,
                    DateAdded = now,
                    SeasonNumber = 1,
                    Languages = new List<int> { 1 }.ToJson()
                });
            });

            var files = db.Query<SubtitleFile198>("SELECT * FROM \"SubtitleFiles\"").ToList();

            files.Should().HaveCount(1);

            files.First().Title.Should().Be(title);
            files.First().Copy.Should().Be(copy);
            files.First().LanguageTags.Should().NotContain("sdh");
            files.First().Language.Should().NotBe(10);
        }
    }

    public class SubtitleFile198
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
        public int Copy { get; set; }
        public string Title { get; set; }
        public List<string> LanguageTags { get; set; }
    }
}
