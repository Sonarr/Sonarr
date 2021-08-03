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
    public class ReplaceCharacterFixture : CoreTest<FileNameBuilder>
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

//        { "\\", "/", "<", ">", "?", "*", ":", "|", "\"" };
//        { "+", "+", "", "", "!", "-", " -", "", "" };

        [TestCase("CSI: Crime Scene Investigation", "CSI - Crime Scene Investigation")]
        [TestCase("Code:Breaker", "Code-Breaker")]
        [TestCase("Back Slash\\", "Back Slash+")]
        [TestCase("Forward Slash/", "Forward Slash+")]
        [TestCase("Greater Than>", "Greater Than")]
        [TestCase("Less Than<", "Less Than")]
        [TestCase("Question Mark?", "Question Mark!")]
        [TestCase("Aster*sk", "Aster-sk")]
        [TestCase("Colon: Two Periods", "Colon - Two Periods")]
        [TestCase("Pipe|", "Pipe")]
        [TestCase("Quotes\"", "Quotes")]
        public void should_replace_illegal_characters(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.StandardEpisodeFormat = "{Series Title}";

            Subject.BuildFileName(new List<Episode> { _episode }, _series, _episodeFile)
                   .Should().Be(expected);
        }
    }
}
