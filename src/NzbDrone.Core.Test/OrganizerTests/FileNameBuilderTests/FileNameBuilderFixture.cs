using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode1;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episode1 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        private void GivenProper()
        {
            _episodeFile.Quality.Revision.Version = 2;
        }

        private void GivenReal()
        {
            _episodeFile.Quality.Revision.Real = 1;
        }

        [Test]
        public void should_replace_Series_space_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_replace_Series_underscore_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series_Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South_Park");
        }

        [Test]
        public void should_replace_Series_dot_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park");
        }

        [Test]
        public void should_replace_Series_dash_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series-Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South-Park");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_all_caps()
        {
            _namingConfig.StandardEpisodeFormat = "{SERIES TITLE}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("SOUTH PARK");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardEpisodeFormat = "{sErIES-tItLE}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(_series.Title.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_series_title_with_all_lower_case()
        {
            _namingConfig.StandardEpisodeFormat = "{series title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("south park");
        }

        [Test]
        public void should_cleanup_Series_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.CleanTitle}";
            _series.Title = "South Park (1997)";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.1997");
        }

        [Test]
        public void should_replace_episode_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("City Sushi");
        }

        [Test]
        public void should_replace_episode_title_if_pattern_has_random_casing()
        {
            _namingConfig.StandardEpisodeFormat = "{ePisOde-TitLe}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("City-Sushi");
        }

        [Test]
        public void should_replace_season_number_with_single_digit()
        {
            _episode1.SeasonNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{season}x{episode}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("1x6");
        }

        [Test]
        public void should_replace_season00_number_with_two_digits()
        {
            _episode1.SeasonNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{season:00}x{episode}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("01x6");
        }

        [Test]
        public void should_replace_episode_number_with_single_digit()
        {
            _episode1.SeasonNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{season}x{episode}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("1x6");
        }

        [Test]
        public void should_replace_episode00_number_with_two_digits()
        {
            _episode1.SeasonNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{season}x{episode:00}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("1x06");
        }

        [Test]
        public void should_replace_quality_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("HDTV-720p");
        }

        [Test]
        public void should_replace_quality_proper_with_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Proper}";
            GivenProper();

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("Proper");
        }

        [Test]
        public void should_replace_quality_real_with_real()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Real}";
            GivenReal();

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("REAL");
        }

        [Test]
        public void should_replace_all_contents_in_pattern()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} [{Quality Title}]";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - S15E06 - City Sushi [HDTV-720p]");
        }

        [TestCase("Some Escaped {{ String", "Some Escaped { String")]
        [TestCase("Some Escaped }} String", "Some Escaped } String")]
        [TestCase("Some Escaped {{Series Title}} String", "Some Escaped {Series Title} String")]
        [TestCase("Some Escaped {{{Series Title}}} String", "Some Escaped {South Park} String")]
        public void should_escape_token_in_format(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_escape_token_in_title()
        {
            _namingConfig.StandardEpisodeFormat = "Some Unescaped {Series Title} String";
            _series.Title = "My {Quality Full} Title";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("Some Unescaped My {Quality Full} Title String");
        }

        [Test]
        public void use_file_name_when_sceneName_is_null()
        {
            _namingConfig.RenameEpisodes = false;
            _episodeFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_episodeFile.RelativePath));
        }

        [Test]
        public void use_path_when_sceneName_and_relative_path_are_null()
        {
            _namingConfig.RenameEpisodes = false;
            _episodeFile.RelativePath = null;
            _episodeFile.Path = @"C:\Test\Unsorted\Series - S01E01 - Test";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_episodeFile.Path));
        }

        [Test]
        public void use_file_name_when_sceneName_is_not_null()
        {
            _namingConfig.RenameEpisodes = false;
            _episodeFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _episodeFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_use_airDate_if_series_isDaily_and_not_a_special()
        {
            _namingConfig.DailyEpisodeFormat = "{Series Title} - {air-date} - {Episode Title}";

            _series.Title = "The Daily Show with Jon Stewart";
            _series.SeriesType = SeriesTypes.Daily;

            _episode1.AirDate = "2012-12-13";
            _episode1.Title = "Kristen Stewart";
            _episode1.SeasonNumber = 1;
            _episode1.EpisodeNumber = 5;

            _episodeFile.SeasonNumber = 1;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("The Daily Show with Jon Stewart - 2012-12-13 - Kristen Stewart");
        }

        [Test]
        public void should_use_standard_if_series_isDaily_special()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";

            _series.Title = "The Daily Show with Jon Stewart";
            _series.SeriesType = SeriesTypes.Daily;

            _episode1.AirDate = "2012-12-13";
            _episode1.Title = "Kristen Stewart";
            _episode1.SeasonNumber = 0;
            _episode1.EpisodeNumber = 5;

            _episodeFile.SeasonNumber = 0;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("The Daily Show with Jon Stewart - S00E05 - Kristen Stewart");
        }

        [Test]
        public void should_set_airdate_to_unknown_if_not_available()
        {
            _namingConfig.DailyEpisodeFormat = "{Series Title} - {Air-Date} - {Episode Title}";

            _series.Title = "The Daily Show with Jon Stewart";
            _series.SeriesType = SeriesTypes.Daily;

            _episode1.AirDate = null;
            _episode1.Title = "Kristen Stewart";
            _episode1.SeasonNumber = 1;
            _episode1.EpisodeNumber = 5;

            _episodeFile.SeasonNumber = 1;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("The Daily Show with Jon Stewart - Unknown - Kristen Stewart");
        }

        [Test]
        public void should_not_clean_episode_title_if_there_is_only_one()
        {
            var title = "City Sushi (1)";
            _episode1.Title = title;

            _namingConfig.StandardEpisodeFormat = "{Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(title);
        }

        [Test]
        public void should_should_replace_release_group()
        {
            _namingConfig.StandardEpisodeFormat = "{Release Group}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(_episodeFile.ReleaseGroup);
        }

        [Test]
        public void should_be_able_to_use_original_title()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - {Original Title}";

            _episodeFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _episodeFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("30 Rock - 30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_trim_periods_from_end_of_episode_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 3;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Part 1.")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Episode> { episode }, new Series { Title = "30 Rock" }, _episodeFile)
                   .Should().Be("30 Rock - S06E06 - Part 1");
        }

        [Test]
        public void should_trim_question_marks_from_end_of_episode_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 3;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Part 1?")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Episode> { episode }, new Series { Title = "30 Rock" }, _episodeFile)
                   .Should().Be("30 Rock - S06E06 - Part 1");
        }

        [Test]
        public void should_replace_double_period_with_single_period()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}";

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Part 1")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Episode> { episode }, new Series { Title = "Chicago P.D." }, _episodeFile)
                   .Should().Be("Chicago.P.D.S06E06.Part.1");
        }

        [Test]
        public void should_replace_triple_period_with_single_period()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}";

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Part 1")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Episode> { episode }, new Series { Title = "Chicago P.D.." }, _episodeFile)
                   .Should().Be("Chicago.P.D.S06E06.Part.1");
        }

        [Test]
        public void should_not_replace_absolute_numbering_when_series_is_not_anime()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}.{absolute:00}.{Episode.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.S15E06.City.Sushi");
        }

        [Test]
        public void should_replace_standard_and_absolute_numbering_when_series_is_anime()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}.{absolute:00}.{Episode.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.S15E06.100.City.Sushi");
        }

        [Test]
        public void should_replace_standard_numbering_when_series_is_anime()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.S15E06.City.Sushi");
        }

        [Test]
        public void should_replace_absolute_numbering_when_series_is_anime()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Series.Title}.{absolute:00}.{Episode.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.100.City.Sushi");
        }

        [Test]
        public void should_replace_duplicate_numbering_individually()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Series.Title}.{season}x{episode:00}.{absolute:000}\\{Series.Title}.S{season:00}E{episode:00}.{absolute:00}.{Episode.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.15x06.100\\South.Park.S15E06.100.City.Sushi".AsOsAgnostic());
        }

        [Test]
        public void should_replace_individual_season_episode_tokens()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} Season {season:0000} Episode {episode:0000}\\{Series.Title}.S{season:00}E{episode:00}.{absolute:00}.{Episode.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park Season 0015 Episode 0006\\South.Park.S15E06.100.City.Sushi".AsOsAgnostic());
        }

        [Test]
        public void should_use_standard_naming_when_anime_episode_has_no_absolute_number()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _episode1.AbsoluteEpisodeNumber = null;

            _namingConfig.StandardEpisodeFormat = "{Series Title} - {season:0}x{episode:00} - {Episode Title}";
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:000} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1, }, _series, _episodeFile)
                   .Should().Be("South Park - 15x06 - City Sushi");
        }

        [Test]
        public void should_include_affixes_if_value_not_empty()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}{_Episode.Title_}{Quality.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.S15E06_City.Sushi_HDTV-720p");
        }

        [Test]
        public void should_not_include_affixes_if_value_empty()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}{_Episode.Title_}";

            _episode1.Title = "";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.S15E06");
        }

        [Test]
        public void should_format_mediainfo_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MEDIAINFO.FULL}";

            _episodeFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioFormat = "dts",
                AudioLanguages = new List<string> { "eng", "spa" },
                Subtitles = new List<string> { "eng", "spa", "ita" }
            };

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.S15E06.City.Sushi.H264.DTS[EN+ES].[EN+ES+IT]");
        }

        [TestCase("nob", "NB")]
        [TestCase("swe", "SV")]
        [TestCase("zho", "ZH")]
        [TestCase("chi", "ZH")]
        [TestCase("fre", "FR")]
        [TestCase("rum", "RO")]
        [TestCase("per", "FA")]
        [TestCase("ger", "DE")]
        [TestCase("cze", "CS")]
        [TestCase("ice", "IS")]
        [TestCase("dut", "NL")]
        [TestCase("nor", "NO")]
        public void should_format_languagecodes_properly(string language, string code)
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MEDIAINFO.FULL}";

            _episodeFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioFormat = "dts",
                AudioChannels = 6,
                AudioLanguages = new List<string> { "eng" },
                Subtitles = new List<string> { language },
                SchemaRevision = 3
            };

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be($"South.Park.S15E06.City.Sushi.H264.DTS.[{code}]");
        }

        [Test]
        public void should_exclude_english_in_mediainfo_audio_language()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MEDIAINFO.FULL}";

            _episodeFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioFormat = "dts",
                AudioLanguages = new List<string> { "eng" },
                Subtitles = new List<string> { "eng", "spa", "ita" }
            };

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.S15E06.City.Sushi.H264.DTS.[EN+ES+IT]");
        }

        [Ignore("not currently supported")]
        [Test]
        public void should_remove_duplicate_non_word_characters()
        {
            _series.Title = "Venture Bros.";
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.{season}x{episode:00}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("Venture.Bros.15x06");
        }

        [Test]
        public void should_use_existing_filename_when_scene_name_is_not_available()
        {
            _namingConfig.RenameEpisodes = true;
            _namingConfig.StandardEpisodeFormat = "{Original Title}";

            _episodeFile.SceneName = null;
            _episodeFile.RelativePath = "existing.file.mkv";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_episodeFile.RelativePath));
        }

        [Test]
        public void should_be_able_to_use_only_original_title()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardEpisodeFormat = "{Original Title}";

            _episodeFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _episodeFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_allow_period_between_season_and_episode()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}.S{season:00}.E{episode:00}.{Episode.Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South.Park.S15.E06.City.Sushi");
        }

        [Test]
        public void should_allow_space_between_season_and_episode()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00} E{episode:00} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - S15 E06 - City Sushi");
        }

        [Test]
        public void should_replace_quality_proper_with_v2_for_anime_v2()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Quality Proper}";

            GivenProper();

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("v2");
        }

        [Test]
        public void should_not_include_quality_proper_when_release_is_not_a_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Title} {Quality Proper}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("HDTV-720p");
        }

        [Test]
        public void should_wrap_proper_in_square_brackets()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} [{Quality Title}] {[Quality Proper]}";

            GivenProper();

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p] [Proper]");
        }

        [Test]
        public void should_not_wrap_proper_in_square_brackets_when_not_a_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} [{Quality Title}] {[Quality Proper]}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_only_when_not_a_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} [{Quality Full}]";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_proper_only_when_a_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} [{Quality Full}]";

            GivenProper();

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p Proper]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_real_when_a_real()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} [{Quality Full}]";
            GivenReal();

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - S15E06 [HDTV-720p REAL]");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_end_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardEpisodeFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}", separator);

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("HDTV-720p");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_middle_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardEpisodeFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}{0}{{Episode{0}Title}}", separator);

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(string.Format("HDTV-720p{0}City{0}Sushi", separator));
        }

        [Test]
        public void should_not_require_a_separator_between_tokens()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "[{Release Group}]{Series.CleanTitle}.{absolute:000}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("[SonarrTest]South.Park.100");
        }

        [Test]
        public void should_be_able_to_use_original_filename_only()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardEpisodeFormat = "{Original Filename}";

            _episodeFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _episodeFile.RelativePath = "30 Rock - S01E01 - Test";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("30 Rock - S01E01 - Test");
        }

        [Test]
        public void should_use_Sonarr_as_release_group_when_not_available()
        {
            _episodeFile.ReleaseGroup = null;
            _namingConfig.StandardEpisodeFormat = "{Release Group}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("Sonarr");
        }

        [TestCase("{Episode Title}{-Release Group}", "City Sushi")]
        [TestCase("{Episode Title}{ Release Group}", "City Sushi")]
        [TestCase("{Episode Title}{ [Release Group]}", "City Sushi")]
        public void should_not_use_Sonarr_as_release_group_if_pattern_has_separator(string pattern, string expectedFileName)
        {
            _episodeFile.ReleaseGroup = null;
            _namingConfig.StandardEpisodeFormat = pattern;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(expectedFileName);
        }

        [TestCase("0SEC")]
        [TestCase("2HD")]
        [TestCase("IMMERSE")]
        public void should_use_existing_casing_for_release_group(string releaseGroup)
        {
            _episodeFile.ReleaseGroup = releaseGroup;
            _namingConfig.StandardEpisodeFormat = "{Release Group}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(releaseGroup);
        }

        [TestCase("en-US")]
        [TestCase("fr-FR")]
        [TestCase("az")]
        [TestCase("tr-TR")]
        public void should_replace_all_tokens_for_different_cultures(string culture)
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

                _episodeFile.ReleaseGroup = null;

                GivenMediaInfoModel(audioLanguages: "eng/deu");

                _namingConfig.StandardEpisodeFormat = "{MediaInfo AudioLanguages}";

                Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                       .Should().Be("[EN+DE]");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        [TestCase("eng", "")]
        [TestCase("eng/deu", "[EN+DE]")]
        public void should_format_audio_languages(string audioLanguages, string expected)
        {
            _episodeFile.ReleaseGroup = null;

            GivenMediaInfoModel(audioLanguages: audioLanguages);

            _namingConfig.StandardEpisodeFormat = "{MediaInfo AudioLanguages}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(expected);
        }

        [TestCase("eng", "[EN]")]
        [TestCase("eng/deu", "[EN+DE]")]
        public void should_format_audio_languages_all(string audioLanguages, string expected)
        {
            _episodeFile.ReleaseGroup = null;

            GivenMediaInfoModel(audioLanguages: audioLanguages);

            _namingConfig.StandardEpisodeFormat = "{MediaInfo AudioLanguagesAll}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(expected);
        }

        [TestCase("eng/deu", "", "[EN+DE]")]
        [TestCase("eng/nld/deu", "", "[EN+NL+DE]")]
        [TestCase("eng/deu", ":DE", "[DE]")]
        [TestCase("eng/nld/deu", ":EN+NL", "[EN+NL]")]
        [TestCase("eng/nld/deu", ":NL+EN", "[NL+EN]")]
        [TestCase("eng/nld/deu", ":-NL", "[EN+DE]")]
        [TestCase("eng/nld/deu", ":DE+", "[DE+-]")]
        [TestCase("eng/nld/deu", ":DE+NO.", "[DE].")]
        [TestCase("eng/nld/deu", ":-EN-", "[NL+DE]-")]
        public void should_format_subtitle_languages_all(string subtitleLanguages, string format, string expected)
        {
            _episodeFile.ReleaseGroup = null;

            GivenMediaInfoModel(subtitles: subtitleLanguages);

            _namingConfig.StandardEpisodeFormat = "{MediaInfo SubtitleLanguages" + format + "}End";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(expected + "End");
        }

        [TestCase(HdrFormat.None, "South.Park.S15E06.City.Sushi")]
        [TestCase(HdrFormat.Hlg10, "South.Park.S15E06.City.Sushi.HDR")]
        [TestCase(HdrFormat.Hdr10, "South.Park.S15E06.City.Sushi.HDR")]
        public void should_include_hdr_for_mediainfo_videodynamicrange_with_valid_properties(HdrFormat hdrFormat, string expectedName)
        {
            _namingConfig.StandardEpisodeFormat =
                "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(hdrFormat: hdrFormat);

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                .Should().Be(expectedName);
        }

        [Test]
        public void should_update_media_info_if_token_configured_and_revision_is_old()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 3);

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_episodeFile, _series), Times.Once());
        }

        [Test]
        public void should_not_update_media_info_if_no_series_path_available()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 3);
            _series.Path = null;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_episodeFile, _series), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_not_configured_and_revision_is_old()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}";

            GivenMediaInfoModel(schemaRevision: 3);

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_episodeFile, _series), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_configured_and_revision_is_current()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 5);

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_episodeFile, _series), Times.Never());
        }

        [Test]
        public void should_not_update_media_info_if_token_configured_and_revision_is_newer()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MediaInfo VideoDynamicRange}";

            GivenMediaInfoModel(schemaRevision: 8);

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile);

            Mocker.GetMock<IUpdateMediaInfo>().Verify(v => v.Update(_episodeFile, _series), Times.Never());
        }

        [TestCase("{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MediaInfo VideoDynamicRange}")]
        [TestCase("{Series.Title}.S{season:00}E{episode:00}.{Episode.Title}.{MediaInfo.VideoDynamicRange}")]
        public void should_use_updated_media_info_if_token_configured_and_revision_is_old(string standardEpisodeFormat)
        {
            _namingConfig.StandardEpisodeFormat = standardEpisodeFormat;

            GivenMediaInfoModel(schemaRevision: 3);

            Mocker.GetMock<IUpdateMediaInfo>()
                .Setup(u => u.Update(_episodeFile, _series))
                .Callback((EpisodeFile e, Series s) => e.MediaInfo = new MediaInfoModel
                {
                    VideoFormat = "AVC",
                    AudioFormat = "DTS",
                    AudioChannels = 6,
                    AudioLanguages = new List<string> { "eng" },
                    Subtitles = new List<string> { "eng", "esp", "ita" },
                    VideoBitDepth = 10,
                    VideoColourPrimaries = "bt2020",
                    VideoTransferCharacteristics = "PQ",
                    VideoHdrFormat = HdrFormat.Pq10,
                    SchemaRevision = 5
                });

            var result = Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile);

            result.Should().EndWith("HDR");
        }

        private void GivenMediaInfoModel(string videoCodec = "h264",
                                         string audioCodec = "dts",
                                         int audioChannels = 6,
                                         int videoBitDepth = 8,
                                         HdrFormat hdrFormat = HdrFormat.None,
                                         string audioLanguages = "eng",
                                         string subtitles = "eng/spa/ita",
                                         int schemaRevision = 5)
        {
            _episodeFile.MediaInfo = new MediaInfoModel
            {
                VideoFormat = videoCodec,
                AudioFormat = audioCodec,
                AudioChannels = audioChannels,
                AudioLanguages = audioLanguages.Split("/").ToList(),
                Subtitles = subtitles.Split("/").ToList(),
                VideoBitDepth = videoBitDepth,
                VideoHdrFormat = hdrFormat,
                SchemaRevision = schemaRevision
            };
        }
    }
}
