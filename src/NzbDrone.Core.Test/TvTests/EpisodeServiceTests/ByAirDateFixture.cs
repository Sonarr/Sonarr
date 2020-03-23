using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeServiceTests
{
    [TestFixture]
    public class ByAirDateFixture : CoreTest<EpisodeService>
    {
        private const int SERIES_ID = 1;
        private const string AIR_DATE = "2014-04-02";

        private Episode CreateEpisode(int seasonNumber, int episodeNumber)
        {
            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.SeriesId = 1)
                                          .With(e => e.SeasonNumber = seasonNumber)
                                          .With(e => e.EpisodeNumber = episodeNumber)
                                          .With(e => e.AirDate = AIR_DATE)
                                          .BuildNew();

            return episode;
        }

        private void GivenEpisodes(params Episode[] episodes)
        {
            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.Find(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(episodes.ToList());
        }

        [Test]
        public void should_throw_when_multiple_regular_episodes_are_found_and_not_part_provided()
        {
            GivenEpisodes(CreateEpisode(1, 1), CreateEpisode(2, 1));

            Assert.Throws<InvalidOperationException>(() => Subject.FindEpisode(SERIES_ID, AIR_DATE, null));
        }

        [Test]
        public void should_return_null_when_finds_no_episode()
        {
            GivenEpisodes();

            Subject.FindEpisode(SERIES_ID, AIR_DATE, null).Should().BeNull();
        }

        [Test]
        public void should_get_episode_when_single_episode_exists_for_air_date()
        {
            GivenEpisodes(CreateEpisode(1, 1));

            Subject.FindEpisode(SERIES_ID, AIR_DATE, null).Should().NotBeNull();
        }

        [Test]
        public void should_get_episode_when_regular_episode_and_special_share_the_same_air_date()
        {
            GivenEpisodes(CreateEpisode(1, 1), CreateEpisode(0, 1));

            Subject.FindEpisode(SERIES_ID, AIR_DATE, null).Should().NotBeNull();
        }

        [Test]
        public void should_get_special_when_its_the_only_episode_for_the_date_provided()
        {
            GivenEpisodes(CreateEpisode(0, 1));

            Subject.FindEpisode(SERIES_ID, AIR_DATE, null).Should().NotBeNull();
        }

        [Test]
        public void should_get_episode_when_two_regular_episodes_share_the_same_air_date_and_part_is_provided()
        {
            var episode1 = CreateEpisode(1, 1);
            var episode2 = CreateEpisode(1, 2);

            GivenEpisodes(episode1, episode2);

            Subject.FindEpisode(SERIES_ID, AIR_DATE, 1).Should().Be(episode1);
            Subject.FindEpisode(SERIES_ID, AIR_DATE, 2).Should().Be(episode2);
        }
    }
}
