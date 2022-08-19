using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class AbsoluteEpisodeFormatFixture : CoreTest<FileNameBuilder>
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
                    .With(s => s.SeriesType = SeriesTypes.Anime)
                    .With(s => s.Title = "Anime Series")
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

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_use_standard_format_if_absolute_format_requires_absolute_episode_number_and_it_is_missing()
        {
            _episode.AbsoluteEpisodeNumber = null;
            _namingConfig.StandardEpisodeFormat = "{Series Title} S{season:00}E{episode:00}";
            _namingConfig.AnimeEpisodeFormat = "{Series Title} {absolute:00} [{ReleaseGroup}]";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("Anime Series S15E06");
        }

        [Test]
        public void should_use_absolute_format_if_absolute_format_requires_absolute_episode_number_and_it_is_available()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} S{season:00}E{episode:00}";
            _namingConfig.AnimeEpisodeFormat = "{Series Title} {absolute:00} [{ReleaseGroup}]";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("Anime Series 100 [SonarrTest]");
        }

        [Test]
        public void should_use_absolute_format_if_absolute_format_does_not_require_absolute_episode_number_and_it_is_not_available()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} S{season:00}E{episode:00}";
            _namingConfig.AnimeEpisodeFormat = "{Series Title} S{season:00}E{episode:00} [{ReleaseGroup}]";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("Anime Series S15E06 [SonarrTest]");
        }
    }
}
