using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class RequiresAbsoluteEpisodeNumberFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private Episode _episode;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.SeriesType = SeriesTypes.Anime)
                    .With(s => s.Title = "South Park")
                    .Build();

            _episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.AbsoluteEpisodeNumber = 100)
                            .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        public void should_return_false_when_absolute_episode_number_is_not_part_of_the_pattern()
        {
            _namingConfig.AnimeEpisodeFormat = "{Series Title} S{season:00}E{episode:00}";
            Subject.RequiresAbsoluteEpisodeNumber().Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_absolute_episode_number_is_part_of_the_pattern()
        {
            _namingConfig.AnimeEpisodeFormat = "{Series Title} {absolute:00}";
            Subject.RequiresAbsoluteEpisodeNumber().Should().BeTrue();
        }
    }
}
