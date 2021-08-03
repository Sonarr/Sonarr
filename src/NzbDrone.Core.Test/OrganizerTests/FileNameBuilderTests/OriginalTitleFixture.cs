using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class OriginalTitleFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "My Series")
                    .Build();

            _episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episodeFile = new EpisodeFile { Id = 5, Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [Test]
        public void should_include_original_title_if_not_current_file_name()
        {
            _episodeFile.SceneName = "my.series.s15e06";
            _episodeFile.RelativePath = "My Series - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - S15E06 - City Sushi [my.series.s15e06]");
        }

        [Test]
        public void should_include_current_filename_if_not_renaming_files()
        {
            _episodeFile.SceneName = "my.series.s15e06";
            _namingConfig.RenameEpisodes = false;

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("my.series.s15e06");
        }

        [Test]
        public void should_include_current_filename_if_not_including_season_and_episode_tokens_for_standard_series()
        {
            _episodeFile.RelativePath = "My Series - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - S15E06 - City Sushi HDTV-720p");
        }

        [Test]
        public void should_include_current_filename_if_not_including_air_date_token_for_daily_series()
        {
            _series.SeriesType = SeriesTypes.Daily;
            _episode.AirDate = "2022-04-28";
            _episodeFile.RelativePath = "My Series - 2022-04-28 - City Sushi";
            _namingConfig.DailyEpisodeFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - 2022-04-28 - City Sushi HDTV-720p");
        }

        [Test]
        public void should_include_current_filename_if_not_including_absolute_episode_number_token_for_anime_series()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _episode.AbsoluteEpisodeNumber = 123;
            _episodeFile.RelativePath = "My Series - 123 - City Sushi";
            _namingConfig.AnimeEpisodeFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - 123 - City Sushi HDTV-720p");
        }

        [Test]
        public void should_not_include_current_filename_if_including_season_and_episode_tokens_for_standard_series()
        {
            _episodeFile.RelativePath = "My Series - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - S15E06");
        }

        [Test]
        public void should_not_include_current_filename_if_including_air_date_token_for_daily_series()
        {
            _series.SeriesType = SeriesTypes.Daily;
            _episode.AirDate = "2022-04-28";
            _episodeFile.RelativePath = "My Series - 2022-04-28 - City Sushi";
            _namingConfig.DailyEpisodeFormat = "{Series Title} - {Air-Date} {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - 2022-04-28");
        }

        [Test]
        public void should_not_include_current_filename_if_including_absolute_episode_number_token_for_anime_series()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _episode.AbsoluteEpisodeNumber = 123;
            _episodeFile.RelativePath = "My Series - 123 - City Sushi";
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:00} {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - 123");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_season_and_episode_tokens_for_standard_series()
        {
            _episodeFile.Id = 0;
            _episodeFile.RelativePath = "My Series - S15E06 - City Sushi";
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - S15E06 [My Series - S15E06 - City Sushi]");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_air_date_token_for_daily_series()
        {
            _series.SeriesType = SeriesTypes.Daily;
            _episode.AirDate = "2022-04-28";
            _episodeFile.Id = 0;
            _episodeFile.RelativePath = "My Series - 2022-04-28 - City Sushi";
            _namingConfig.DailyEpisodeFormat = "{Series Title} - {Air-Date} {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - 2022-04-28 [My Series - 2022-04-28 - City Sushi]");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_absolute_episode_number_token_for_anime_series()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _episode.AbsoluteEpisodeNumber = 123;
            _episodeFile.Id = 0;
            _episodeFile.RelativePath = "My Series - 123 - City Sushi";
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:00} {[Original Title]}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("My Series - 123 [My Series - 123 - City Sushi]");
        }
    }
}
