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

    public class CustomFormatsFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode1;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        private List<CustomFormat> _customFormats;

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

            _customFormats = new List<CustomFormat>()
            {
                new CustomFormat()
                {
                    Name = "INTERNAL",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat()
                {
                    Name = "AMZN",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat()
                {
                    Name = "NAME WITH SPACES",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat()
                {
                    Name = "NotIncludedFormat",
                    IncludeCustomFormatWhenRenaming = false
                }
            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [TestCase("{Custom Formats}", "INTERNAL AMZN NAME WITH SPACES")]
        public void should_replace_custom_formats(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile, customFormats: _customFormats)
                   .Should().Be(expected);
        }

        [TestCase("{Custom Formats}", "")]
        public void should_replace_custom_formats_with_no_custom_formats(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile, customFormats: new List<CustomFormat>())
                   .Should().Be(expected);
        }

        [TestCase("{Custom Format}", "")]
        [TestCase("{Custom Format:INTERNAL}", "INTERNAL")]
        [TestCase("{Custom Format:AMZN}", "AMZN")]
        [TestCase("{Custom Format:NAME WITH SPACES}", "NAME WITH SPACES")]
        [TestCase("{Custom Format:DOESNOTEXIST}", "")]
        [TestCase("{Custom Format:INTERNAL} - {Custom Format:AMZN}", "INTERNAL - AMZN")]
        [TestCase("{Custom Format:AMZN} - {Custom Format:INTERNAL}", "AMZN - INTERNAL")]
        public void should_replace_custom_format(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile, customFormats: _customFormats)
                   .Should().Be(expected);
        }

        [TestCase("{Custom Format}", "")]
        [TestCase("{Custom Format:INTERNAL}", "")]
        [TestCase("{Custom Format:AMZN}", "")]
        public void should_replace_custom_format_with_no_custom_formats(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile, customFormats: new List<CustomFormat>())
                   .Should().Be(expected);
        }
    }
}
