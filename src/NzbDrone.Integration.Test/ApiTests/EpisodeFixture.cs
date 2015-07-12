using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Series;
using System.Linq;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class EpisodeFixture : IntegrationTest
    {
        private SeriesResource series;

        [SetUp]
        public void Setup()
        {
            series = GivenSeriesWithEpisodes();
        }

        private SeriesResource GivenSeriesWithEpisodes()
        {
            var newSeries = Series.Lookup("archer").Single(c => c.TvdbId == 110381);

            newSeries.ProfileId = 1;
            newSeries.LanguageProfileId = 1;
            newSeries.Path = @"C:\Test\Archer".AsOsAgnostic();

            newSeries = Series.Post(newSeries);

            WaitForCompletion(() => Episodes.GetEpisodesInSeries(newSeries.Id).Count > 0);

            return newSeries;
        }

        [Test]
        public void should_be_able_to_get_all_episodes_in_series()
        {
            Episodes.GetEpisodesInSeries(series.Id).Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_get_a_single_episode()
        {
            var episodes = Episodes.GetEpisodesInSeries(series.Id);

            Episodes.Get(episodes.First().Id).Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_set_monitor_status()
        {
            var episodes = Episodes.GetEpisodesInSeries(series.Id);
            var updatedEpisode = episodes.First();
            updatedEpisode.Monitored = false;

            Episodes.Put(updatedEpisode).Monitored.Should().BeFalse();
        }


        [TearDown]
        public void TearDown()
        {
            Series.Delete(series.Id);
            Thread.Sleep(2000);
        }
    }
}
