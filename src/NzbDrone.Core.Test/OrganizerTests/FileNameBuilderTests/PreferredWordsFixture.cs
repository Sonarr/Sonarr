using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class PreferredWordsFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode1;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        private PreferredWordMatchResults _preferredWords;

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

            _preferredWords = new PreferredWordMatchResults()
            {
                All = new List<string>()
                {
                    "x265",
                    "extended"
                },
                ByReleaseProfile = new Dictionary<string, List<string>>()
                {
                    {
                        "CodecProfile",
                        new List<string>()
                        {
                            "x265"
                        }
                    },
                    {
                        "EditionProfile",
                        new List<string>()
                        {
                            "extended"
                        }
                    }
                }
            };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [TestCase("{Preferred Words}", "x265 extended")]
        [TestCase("{Preferred Words:CodecProfile}", "x265")]
        [TestCase("{Preferred Words:EditionProfile}", "extended")]
        [TestCase("{Preferred Words:CodecProfile} - {PreferredWords:EditionProfile}", "x265 - extended")]
        public void should_replace_PreferredWords(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile, preferredWords: _preferredWords)
                   .Should().Be(expected);
        }

        [TestCase("{Preferred Words:}", "{Preferred Words:}")]
        public void should_not_replace_PreferredWords(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile, preferredWords: _preferredWords)
                   .Should().Be(expected);
        }

        [TestCase("{Preferred Words:NonexistentProfile}", "")]
        public void should_replace_PreferredWords_with_empty_string(string format, string expected)
        {
            _namingConfig.StandardEpisodeFormat = format;

            Subject.BuildFileName(new List<Episode> { _episode1 }, _series, _episodeFile, preferredWords: _preferredWords)
                   .Should().Be(expected);
        }
    }
}
