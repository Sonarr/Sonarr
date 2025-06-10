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
    public class QualityProperFixture : CoreTest<FileNameBuilder>
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
                    .Build();

            _episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

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
        public void should_get_quality_proper_without_quality()
        {
            _episodeFile.Quality.Revision = new Revision();
            _namingConfig.StandardEpisodeFormat = "{Quality Proper}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("");
        }

        [Test]
        public void should_get_quality_proper()
        {
            _episodeFile.Quality.Revision = new Revision(2);
            _namingConfig.StandardEpisodeFormat = "{Quality Proper}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                .Should().Be("Proper");
        }

        [Test]
        public void should_get_quality_repack()
        {
            _episodeFile.Quality.Revision = new Revision(2, 0, 0, true);
            _namingConfig.StandardEpisodeFormat = "{Quality Proper}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                .Should().Be("Repack");
        }

        [Test]
        public void should_get_quality_proper_and_repack()
        {
            _episodeFile.Quality.Revision = new Revision(3, 0, 1, true);
            _namingConfig.StandardEpisodeFormat = "{Quality Proper}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                .Should().Be("Proper Repack");
        }

        [Test]
        public void should_get_quality_proper_for_anime()
        {
            _series.SeriesType = SeriesTypes.Anime;
            _episodeFile.Quality.Revision = new Revision(3, 0, 1, true);
            _namingConfig.AnimeEpisodeFormat = "{Quality Proper}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                .Should().Be("v3");
        }
    }
}
