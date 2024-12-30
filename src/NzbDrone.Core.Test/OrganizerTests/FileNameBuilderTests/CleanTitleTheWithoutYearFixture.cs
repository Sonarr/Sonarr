using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using Workarr.CustomFormats;
using Workarr.MediaFiles;
using Workarr.Organizer;
using Workarr.Qualities;
using Workarr.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class CleanTitleTheWithoutYearFixture : CoreTest<FileNameBuilder>
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

        [TestCase("The Mist", 2018, "Mist, The")]
        [TestCase("The Rat Pack (A&E)", 1999, "Rat Pack, The AandE")]
        [TestCase("The Climax: I (Almost) Got Away With It (2016)", 2016, "Climax I Almost Got Away With It, The")]
        [TestCase("A", 2017, "A")]
        public void should_get_expected_title_back(string title, int year, string expected)
        {
            _series.Title = title;
            _series.Year = year;
            _namingConfig.StandardEpisodeFormat = "{Series CleanTitleTheWithoutYear}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_not_include_0_for_year()
        {
            _series.Title = "The Alienist";
            _series.Year = 0;
            _namingConfig.StandardEpisodeFormat = "{Series CleanTitleTheWithoutYear}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be("Alienist, The");
        }
    }
}
