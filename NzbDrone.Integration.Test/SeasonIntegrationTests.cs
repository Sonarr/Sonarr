using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.Series;
using System.Linq;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class SeasonIntegrationTests : IntegrationTest
    {
        private SeriesResource GivenSeriesWithEpisodes()
        {
            var series = Series.Lookup("archer").First();

            series.QualityProfileId = 1;
            series.Path = @"C:\Test\Archer";

            series = Series.Post(series);

            while (true)
            {
                if (Seasons.GetSeasonsInSeries(series.Id).Count > 0)
                {
                    return series;
                }

                Thread.Sleep(1000);
            }
        }

        [Test]
        public void should_be_able_to_get_all_seasons_in_series()
        {
            var series = GivenSeriesWithEpisodes();
            Seasons.GetSeasonsInSeries(series.Id).Count.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_be_able_to_get_a_single_season()
        {
            var series = GivenSeriesWithEpisodes();
            var seasons = Seasons.GetSeasonsInSeries(series.Id);

            Seasons.Get(seasons.First().Id).Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_set_monitor_status_via_api()
        {
            var series = GivenSeriesWithEpisodes();
            var seasons = Seasons.GetSeasonsInSeries(series.Id);
            var updatedSeason = seasons.First();
            updatedSeason.Monitored = false;

            Seasons.Put(updatedSeason).Monitored.Should().BeFalse();
        }
    }
}