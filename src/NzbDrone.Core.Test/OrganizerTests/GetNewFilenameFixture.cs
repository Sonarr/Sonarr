using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode1;
        private Episode _episode2;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();


            _namingConfig = new NamingConfig();
            _namingConfig.RenameEpisodes = true;


            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episode1 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            _episode2 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 7)
                            .Build();

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p) };
        }

        private void GivenProper()
        {
            _episodeFile.Quality.Proper = true;
        }

        [Test]
        public void should_replace_Series_space_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title}";

            Subject.BuildFilename(new List<Episode> {_episode1}, _series, _episodeFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_replace_Series_underscore_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series_Title}";

            Subject.BuildFilename(new List<Episode> {_episode1}, _series, _episodeFile)
                   .Should().Be("South_Park");
        }

        [Test]
        public void should_replace_Series_dot_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series.Title}";

            Subject.BuildFilename(new List<Episode> {_episode1}, _series, _episodeFile)
                   .Should().Be("South.Park");
        }

        [Test]
        public void should_replace_Series_dash_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series-Title}";

            Subject.BuildFilename(new List<Episode> {_episode1}, _series, _episodeFile)
                   .Should().Be("South-Park");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_all_caps()
        {
            _namingConfig.StandardEpisodeFormat = "{SERIES TITLE}";

            Subject.BuildFilename(new List<Episode> {_episode1}, _series, _episodeFile)
                   .Should().Be("SOUTH PARK");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardEpisodeFormat = "{sErIES-tItLE}";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(_series.Title.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_series_title_with_all_lower_case()
        {
            _namingConfig.StandardEpisodeFormat = "{series title}";

            Subject.BuildFilename(new List<Episode> {_episode1}, _series, _episodeFile)
                   .Should().Be("south park");
        }

        [Test]
        public void should_replace_episode_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Episode Title}";

            Subject.BuildFilename(new List<Episode> {_episode1}, _series, _episodeFile)
                   .Should().Be("City Sushi");
        }

        [Test]
        public void should_replace_episode_title_if_pattern_has_random_casing()
        {
            _namingConfig.StandardEpisodeFormat = "{ePisOde-TitLe}";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("City-Sushi");
        }

        [Test]
        public void should_replace_season_number_with_single_digit()
        {
            _episode1.SeasonNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{season}x{episode}";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("1x6");
        }

        [Test]
        public void should_replace_season00_number_with_two_digits()
        {
            _episode1.SeasonNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{season:00}x{episode}";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("01x6");
        }

        [Test]
        public void should_replace_episode_number_with_single_digit()
        {
            _episode1.SeasonNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{season}x{episode}";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("1x6");
        }

        [Test]
        public void should_replace_episode00_number_with_two_digits()
        {
            _episode1.SeasonNumber = 1;
            _namingConfig.StandardEpisodeFormat = "{season}x{episode:00}";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("1x06");
        }

        [Test]
        public void should_replace_quality_title()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Title}";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("HDTV-720p");
        }

        [Test]
        public void should_replace_quality_title_with_proper()
        {
            _namingConfig.StandardEpisodeFormat = "{Quality Title}";
            _episodeFile.Quality.Proper = true;

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("HDTV-720p Proper");
        }

        [Test]
        public void should_replace_all_contents_in_pattern()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} [{Quality Title}]";

            Subject.BuildFilename(new List<Episode> {_episode1}, _series, _episodeFile)
                   .Should().Be("South Park - S15E06 - City Sushi [HDTV-720p]");
        }

        [Test]
        public void use_file_name_when_sceneName_is_null()
        {
            _namingConfig.RenameEpisodes = false;
            _episodeFile.Path = @"C:\Test\TV\30 Rock - S01E01 - Test";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_episodeFile.Path));
        }

        [Test]
        public void use_file_name_when_sceneName_is_not_null()
        {
            _namingConfig.RenameEpisodes = false;
            _episodeFile.SceneName = "30.Rock.S01E01.xvid-LOL";
            _episodeFile.Path = @"C:\Test\TV\30 Rock - S01E01 - Test";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("30.Rock.S01E01.xvid-LOL");
        }

        [Test]
        public void should_only_have_one_episodeTitle_when_episode_titles_are_the_same()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 3;

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hey, Baby, What's Wrong? (1)")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            var episode2 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Hey, Baby, What's Wrong? (2)")
                            .With(e => e.SeasonNumber = 6)
                            .With(e => e.EpisodeNumber = 7)
                            .Build();


            Subject.BuildFilename(new List<Episode> {episode2, episode}, new Series {Title = "30 Rock"}, _episodeFile)
                   .Should().Be("30 Rock - S06E06-E07 - Hey, Baby, What's Wrong!");
        }

        [Test]
        public void should_have_two_episodeTitles_when_episode_titles_are_not_the_same()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 3;

            _episode1.Title = "Hello";
            _episode2.Title = "World";

            Subject.BuildFilename(new List<Episode> {_episode1, _episode2}, _series, _episodeFile)
                   .Should().Be("South Park - S15E06-E07 - Hello + World");
        }

        [Test]
        public void should_use_airDate_if_series_isDaily()
        {
            _namingConfig.DailyEpisodeFormat = "{Series Title} - {air-date} - {Episode Title}";

            _series.Title = "The Daily Show with Jon Stewart";
            _series.SeriesType = SeriesTypes.Daily;

            _episode1.AirDate = "2012-12-13";
            _episode1.Title = "Kristen Stewart";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("The Daily Show with Jon Stewart - 2012-12-13 - Kristen Stewart");
        }

        [Test]
        public void should_set_airdate_to_unknown_if_not_available()
        {
            _namingConfig.DailyEpisodeFormat = "{Series Title} - {Air-Date} - {Episode Title}";

            _series.Title = "The Daily Show with Jon Stewart";
            _series.SeriesType = SeriesTypes.Daily;

            _episode1.AirDate = null;
            _episode1.Title = "Kristen Stewart";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("The Daily Show with Jon Stewart - Unknown - Kristen Stewart");
        }

        [Test]
        public void should_format_extend_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 0;

            Subject.BuildFilename(new List<Episode> {_episode1, _episode2}, _series, _episodeFile)
                .Should().Be("South Park - S15E06-07 - City Sushi");
        }

        [Test]
        public void should_format_duplicate_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 1;

            Subject.BuildFilename(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06 - S15E07 - City Sushi");
        }

        [Test]
        public void should_format_repeat_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 2;

            Subject.BuildFilename(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06E07 - City Sushi");
        }

        [Test]
        public void should_format_scene_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 3;

            Subject.BuildFilename(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06-E07 - City Sushi");
        }

        [Test]
        public void should_not_clean_episode_title_if_there_is_only_one()
        {
            var title = "City Sushi (1)";
            _episode1.Title = title;

            _namingConfig.StandardEpisodeFormat = "{Episode Title}";

            Subject.BuildFilename(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be(title);
        }
    }
}