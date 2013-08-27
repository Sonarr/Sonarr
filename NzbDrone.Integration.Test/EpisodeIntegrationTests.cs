using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Series;
using System.Linq;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class EpisodeIntegrationTests : IntegrationTest
    {
        private SeriesResource GivenSeriesWithEpisodes()
        {
            var series = Series.Lookup("archer").First();

            series.QualityProfileId = 1;
            series.Path = @"C:\Test\Archer";

            series = Series.Post(series);

            while (true)
            {
                if (Episodes.GetEpisodesInSeries(series.Id).Count > 0)
                {
                    return series;
                }

                Thread.Sleep(1000);
            }
        }

        [Test]
        public void should_be_able_to_get_all_episodes_in_series()
        {
            var series = GivenSeriesWithEpisodes();
            Episodes.GetEpisodesInSeries(series.Id).Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_get_a_single_episode()
        {
            var series = GivenSeriesWithEpisodes();
            var episodes = Episodes.GetEpisodesInSeries(series.Id);

            Episodes.Get(episodes.First().Id).Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_set_monitor_status_via_api()
        {
            var series = GivenSeriesWithEpisodes();
            var episodes = Episodes.GetEpisodesInSeries(series.Id);
            var updatedEpisode = episodes.First();
            updatedEpisode.Monitored = false;

            Episodes.Put(updatedEpisode).Monitored.Should().BeFalse();
        }
    }
}