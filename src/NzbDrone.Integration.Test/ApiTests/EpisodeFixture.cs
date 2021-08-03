using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;
using Sonarr.Api.V3.Series;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class EpisodeFixture : IntegrationTest
    {
        private SeriesResource _series;

        [SetUp]
        public void Setup()
        {
            _series = GivenSeriesWithEpisodes();
        }

        private SeriesResource GivenSeriesWithEpisodes()
        {
            var newSeries = Series.Lookup("archer").Single(c => c.TvdbId == 110381);

            newSeries.QualityProfileId = 1;
            newSeries.LanguageProfileId = 1;
            newSeries.Path = @"C:\Test\Archer".AsOsAgnostic();

            newSeries = Series.Post(newSeries);

            WaitForCompletion(() => Episodes.GetEpisodesInSeries(newSeries.Id).Count > 0);

            return newSeries;
        }

        [Test]
        public void should_be_able_to_get_all_episodes_in_series()
        {
            Episodes.GetEpisodesInSeries(_series.Id).Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_get_a_single_episode()
        {
            var episodes = Episodes.GetEpisodesInSeries(_series.Id);

            Episodes.Get(episodes.First().Id).Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_set_monitor_status()
        {
            var episodes = Episodes.GetEpisodesInSeries(_series.Id);
            var updatedEpisode = episodes.First();
            updatedEpisode.Monitored = false;

            Episodes.Put(updatedEpisode).Monitored.Should().BeFalse();
        }

        [TearDown]
        public void TearDown()
        {
            Series.Delete(_series.Id);
            Thread.Sleep(2000);
        }
    }
}
