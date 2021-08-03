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

    public class MultiEpisodeFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode1;
        private Episode _episode2;
        private Episode _episode3;
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

            _episode2 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 7)
                            .With(e => e.AbsoluteEpisodeNumber = 101)
                            .Build();

            _episode3 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 8)
                            .With(e => e.AbsoluteEpisodeNumber = 102)
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

        [Test]
        public void should_replace_Series_space_Title()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_format_extend_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 0;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06-07 - City Sushi");
        }

        [Test]
        public void should_format_duplicate_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 1;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06 - S15E07 - City Sushi");
        }

        [Test]
        public void should_format_repeat_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 2;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06E07 - City Sushi");
        }

        [Test]
        public void should_format_scene_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 3;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06-E07 - City Sushi");
        }

        [Test]
        public void should_use_dash_as_separator_when_multi_episode_style_is_extend_for_anime()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:000} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                   .Should().Be("South Park - 100-101 - City Sushi");
        }

        [Test]
        public void should_duplicate_absolute_pattern_when_multi_episode_style_is_duplicate()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.MultiEpisodeStyle = (int)MultiEpisodeStyle.Duplicate;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:000} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2, _episode3 }, _series, _episodeFile)
                   .Should().Be("South Park - 100 - 101 - 102 - City Sushi");
        }

        [Test]
        public void should_get_proper_filename_when_multi_episode_is_duplicated_and_bracket_follows_pattern()
        {
            _namingConfig.StandardEpisodeFormat =
                "{Series Title} - S{season:00}E{episode:00} - ({Quality Title}, {MediaInfo Full}, {Release Group}) - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = (int)MultiEpisodeStyle.Duplicate;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                   .Should().Be("South Park - S15E06 - S15E07 - (HDTV-720p, , SonarrTest) - City Sushi");
        }

        [Test]
        public void should_format_range_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 4;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2, _episode3 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06-08 - City Sushi");
        }

        [Test]
        public void should_format_range_multi_episode_anime_properly()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.MultiEpisodeStyle = 4;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:000} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2, _episode3 }, _series, _episodeFile)
                   .Should().Be("South Park - 100-102 - City Sushi");
        }

        [Test]
        public void should_format_repeat_multi_episode_anime_properly()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.MultiEpisodeStyle = 2;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:000} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2, _episode3 }, _series, _episodeFile)
                   .Should().Be("South Park - 100-101-102 - City Sushi");
        }

        [Test]
        public void should_format_single_episode_with_range_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 4;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_format_single_anime_episode_with_range_multi_episode_properly()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.MultiEpisodeStyle = 4;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:000} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - 100 - City Sushi");
        }

        [Test]
        public void should_default_to_dash_when_serparator_is_not_set_for_absolute_number()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.MultiEpisodeStyle = (int)MultiEpisodeStyle.Duplicate;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {season}x{episode:00} - [{absolute:000}] - {Episode Title} - {Quality Title}";

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                   .Should().Be("South Park - 15x06 - 15x07 - [100-101] - City Sushi - HDTV-720p");
        }

        [Test]
        public void should_format_prefixed_range_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 5;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2, _episode3 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06-E08 - City Sushi");
        }

        [Test]
        public void should_format_prefixed_range_multi_episode_anime_properly()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.MultiEpisodeStyle = 5;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:000} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2, _episode3 }, _series, _episodeFile)
                   .Should().Be("South Park - 100-102 - City Sushi");
        }

        [Test]
        public void should_format_single_episode_with_prefixed_range_multi_episode_properly()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 5;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_format_single_anime_episode_with_prefixed_range_multi_episode_properly()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _namingConfig.MultiEpisodeStyle = 5;
            _namingConfig.AnimeEpisodeFormat = "{Series Title} - {absolute:000} - {Episode Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("South Park - 100 - City Sushi");
        }

        [Test]
        public void should_format_prefixed_range_multi_episode_using_episode_separator()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - {season:0}x{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 5;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2, _episode3 }, _series, _episodeFile)
                .Should().Be("South Park - 15x06-x08 - City Sushi");
        }
    }
}
