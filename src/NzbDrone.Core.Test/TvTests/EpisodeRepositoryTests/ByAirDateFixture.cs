using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class ByAirDateFixture : DbTest<EpisodeRepository, Episode>
    {
        private const int SERIES_ID = 1;
        private const string AIR_DATE = "2014-04-02";

        private void GivenEpisode(int seasonNumber)
        {
            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.SeriesId = 1)
                                          .With(e => e.SeasonNumber = seasonNumber)
                                          .With(e => e.AirDate = AIR_DATE)
                                          .BuildNew();

            Db.Insert(episode);
        }

        [Test]
        public void should_throw_when_multiple_regular_episodes_are_found()
        {
            GivenEpisode(1);
            GivenEpisode(2);

            Assert.Throws<InvalidOperationException>(() => Subject.Get(SERIES_ID, AIR_DATE));
            Assert.Throws<InvalidOperationException>(() => Subject.Find(SERIES_ID, AIR_DATE));
        }

        [Test]
        public void should_throw_when_get_finds_no_episode()
        {
            Assert.Throws<InvalidOperationException>(() => Subject.Get(SERIES_ID, AIR_DATE));
        }

        [Test]
        public void should_get_episode_when_single_episode_exists_for_air_date()
        {
            GivenEpisode(1);

            Subject.Get(SERIES_ID, AIR_DATE).Should().NotBeNull();
            Subject.Find(SERIES_ID, AIR_DATE).Should().NotBeNull();
        }

        [Test]
        public void should_get_episode_when_regular_episode_and_special_share_the_same_air_date()
        {
            GivenEpisode(1);
            GivenEpisode(0);

            Subject.Get(SERIES_ID, AIR_DATE).Should().NotBeNull();
            Subject.Find(SERIES_ID, AIR_DATE).Should().NotBeNull();
        }

        [Test]
        public void should_get_special_when_its_the_only_episode_for_the_date_provided()
        {
            GivenEpisode(0);

            Subject.Get(SERIES_ID, AIR_DATE).Should().NotBeNull();
            Subject.Find(SERIES_ID, AIR_DATE).Should().NotBeNull();
        }
    }
}
