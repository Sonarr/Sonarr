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
    public class TitleTheFixture : CoreTest<FileNameBuilder>
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
                    .With(s => s.Title = "South Park")
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
        }

        [TestCase("The Mist", "Mist, The")]
        [TestCase("A Place to Call Home", "Place to Call Home, A")]
        [TestCase("An Adventure in Space and Time", "Adventure in Space and Time, An")]
        //[TestCase("", "")]
        public void should_get_expected_title_back(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.StandardEpisodeFormat = "{Series TitleThe}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be(expected);
        }

        [TestCase("A")]
        [TestCase("Anne")]
        [TestCase("Theodore")]
        [TestCase("3%")]
        public void should_not_change_title(string title)
        {
            _series.Title = title;
            _namingConfig.StandardEpisodeFormat = "{Series TitleThe}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be(title);
        }
    }
}
