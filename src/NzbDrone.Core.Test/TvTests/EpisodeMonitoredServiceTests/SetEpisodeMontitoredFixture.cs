using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeMonitoredServiceTests
{
    [TestFixture]
    public class SetEpisodeMontitoredFixture : CoreTest<EpisodeMonitoredService>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Status = SeriesStatusType.Continuing)
                                     .With(s => s.Seasons = new List<Season>
                                                            {
                                                                new Season { SeasonNumber = 0, Monitored = true },
                                                                new Season { SeasonNumber = 1, Monitored = true },
                                                                new Season { SeasonNumber = 2, Monitored = true }
                                                            })
                                     .Build();

            // By default no seasons have monitored episodes after the update; individual tests override.
            GivenMonitoredSeasons();
        }

        private void GivenMonitoredSeasons(params int[] seasonNumbers)
        {
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetMonitoredSeasonNumbers(It.IsAny<int>()))
                  .Returns(seasonNumbers.ToList());
        }

        [Test]
        public void should_update_series_without_changing_episodes_when_options_are_null()
        {
            Subject.SetEpisodeMonitoredStatus(_series, null);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeries(It.IsAny<int>(), It.IsAny<MonitorTypes>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_do_nothing_when_skip()
        {
            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.Skip });

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeries(It.IsAny<int>(), It.IsAny<MonitorTypes>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_apply_monitored_status_with_first_and_last_season()
        {
            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.Missing });

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeries(_series.Id, MonitorTypes.Missing, 1, 2), Times.Once());
        }

        [Test]
        public void should_monitor_last_season_when_monitoring_all()
        {
            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.All });

            VerifySeasonMonitored(2);
        }

        [Test]
        public void should_monitor_last_season_when_monitoring_future_and_series_continuing()
        {
            _series.Status = SeriesStatusType.Continuing;

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.Future });

            VerifySeasonMonitored(2);
        }

        [Test]
        public void should_monitor_last_season_when_monitoring_future_and_series_upcoming()
        {
            _series.Status = SeriesStatusType.Upcoming;

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.Future });

            VerifySeasonMonitored(2);
        }

        [Test]
        public void should_not_monitor_last_season_when_monitoring_future_and_series_ended()
        {
            _series.Status = SeriesStatusType.Ended;

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.Future });

            VerifySeasonNotMonitored(2);
        }

        [Test]
        public void should_not_monitor_first_season_when_monitoring_pilot()
        {
            // The pilot episode in the first season is monitored, but the season itself must not be.
            GivenMonitoredSeasons(1);

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.Pilot });

            VerifySeasonNotMonitored(1);
        }

        [Test]
        public void should_monitor_season_with_monitored_episodes()
        {
            GivenMonitoredSeasons(1);

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.Missing });

            VerifySeasonMonitored(1);
            VerifySeasonNotMonitored(2);
        }

        [Test]
        public void should_unmonitor_seasons_without_monitored_episodes()
        {
            GivenMonitoredSeasons();

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.None });

            VerifySeasonNotMonitored(0);
            VerifySeasonNotMonitored(1);
            VerifySeasonNotMonitored(2);
        }

        [Test]
        public void should_monitor_specials_season_when_specials_have_monitored_episodes()
        {
            GivenMonitoredSeasons(0);

            Subject.SetEpisodeMonitoredStatus(_series, new MonitoringOptions { Monitor = MonitorTypes.MonitorSpecials });

            VerifySeasonMonitored(0);
        }

        private void VerifySeasonMonitored(int seasonNumber)
        {
            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Seasons.Single(n => n.SeasonNumber == seasonNumber).Monitored), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        private void VerifySeasonNotMonitored(int seasonNumber)
        {
            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => !s.Seasons.Single(n => n.SeasonNumber == seasonNumber).Monitored), It.IsAny<bool>(), It.IsAny<bool>()));
        }
    }
}
