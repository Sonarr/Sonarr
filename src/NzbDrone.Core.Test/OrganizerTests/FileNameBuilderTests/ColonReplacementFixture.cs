using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
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
    public class ColonReplacementFixture : CoreTest<FileNameBuilder>
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
                    .With(s => s.Title = "CSI: Vegas")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episode1 = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "What Happens in Vegas")
                            .With(e => e.SeasonNumber = 1)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_replace_colon_followed_by_space_with_space_dash_space_by_default()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title}";

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                   .Should().Be("CSI - Vegas");
        }

        [TestCase("CSI: Vegas", ColonReplacementFormat.Smart, "CSI - Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.Dash, "CSI- Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.Delete, "CSI Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.SpaceDash, "CSI - Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.SpaceDashSpace, "CSI - Vegas")]
        public void should_replace_colon_followed_by_space_with_expected_result(string seriesName, ColonReplacementFormat replacementFormat, string expected)
        {
            _series.Title = seriesName;
            _namingConfig.StandardEpisodeFormat = "{Series Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                .Should().Be(expected);
        }

        [TestCase("Series:Title", ColonReplacementFormat.Smart, "Series-Title")]
        [TestCase("Series:Title", ColonReplacementFormat.Dash, "Series-Title")]
        [TestCase("Series:Title", ColonReplacementFormat.Delete, "SeriesTitle")]
        [TestCase("Series:Title", ColonReplacementFormat.SpaceDash, "Series -Title")]
        [TestCase("Series:Title", ColonReplacementFormat.SpaceDashSpace, "Series - Title")]
        public void should_replace_colon_with_expected_result(string seriesName, ColonReplacementFormat replacementFormat, string expected)
        {
            _series.Title = seriesName;
            _namingConfig.StandardEpisodeFormat = "{Series Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile)
                .Should().Be(expected);
        }
    }
}
