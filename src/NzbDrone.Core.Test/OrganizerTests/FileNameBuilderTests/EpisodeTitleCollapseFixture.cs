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
    public class EpisodeTitleCollapseFixture : CoreTest<FileNameBuilder>
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

        [TestCase("Hey, Baby, What's Wrong (1)", "Hey, Baby, What's Wrong (2)", "Hey, Baby, What's Wrong")]
        [TestCase("Meet the Guys and Girls of Cycle 20 Part 1", "Meet the Guys and Girls of Cycle 20 Part 2", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 Part1", "Meet the Guys and Girls of Cycle 20 Part2", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 Part01", "Meet the Guys and Girls of Cycle 20 Part02", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 Part 01", "Meet the Guys and Girls of Cycle 20 Part 02", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 part 1", "Meet the Guys and Girls of Cycle 20 part 2", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 pt 1", "Meet the Guys and Girls of Cycle 20 pt 2", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 pt. 1", "Meet the Guys and Girls of Cycle 20 pt. 2", "Meet the Guys and Girls of Cycle 20")]
        public void should_collapse_episode_titles_when_episode_titles_are_the_same(string title1, string title2, string expected)
        {
            _namingConfig.StandardEpisodeFormat = "{Episode Title}";

            _episode1.Title = title1;
            _episode2.Title = title2;

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_not_collapse_episode_titles_when_episode_titles_are_not_the_same()
        {
            _namingConfig.StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title}";
            _namingConfig.MultiEpisodeStyle = 3;

            _episode1.Title = "Hello";
            _episode2.Title = "World";

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                   .Should().Be("South Park - S15E06-E07 - Hello + World");
        }

        [Test]
        public void should_not_collaspe_when_result_is_empty()
        {
            _namingConfig.StandardEpisodeFormat = "{Episode Title}";

            _episode1.Title = "Part 1";
            _episode2.Title = "Part 2";

            Subject.BuildFileName(new List<Episode> { _episode1, _episode2 }, _series, _episodeFile)
                   .Should().Be("Part 1 + Part 2");
        }
    }
}
