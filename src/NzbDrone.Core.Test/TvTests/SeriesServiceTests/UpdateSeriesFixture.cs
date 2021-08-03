using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.SeriesServiceTests
{
    [TestFixture]
    public class UpdateSeriesFixture : CoreTest<SeriesService>
    {
        private Series _fakeSeries;
        private Series _existingSeries;

        [SetUp]
        public void Setup()
        {
            _fakeSeries = Builder<Series>.CreateNew().Build();
            _existingSeries = Builder<Series>.CreateNew().Build();

            _fakeSeries.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = true },
                new Season { SeasonNumber = 2, Monitored = true }
            };

            _existingSeries.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = true },
                new Season { SeasonNumber = 2, Monitored = true }
            };
        }

        private void GivenExistingSeries()
        {
            Mocker.GetMock<ISeriesRepository>()
                  .Setup(s => s.Get(It.IsAny<int>()))
                  .Returns(_existingSeries);
        }

        [Test]
        public void should_not_update_episodes_if_season_hasnt_changed()
        {
            GivenExistingSeries();

            Subject.UpdateSeries(_fakeSeries);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_update_series_when_it_changes()
        {
            GivenExistingSeries();
            var seasonNumber = 1;
            var monitored = false;

            _fakeSeries.Seasons.Single(s => s.SeasonNumber == seasonNumber).Monitored = monitored;

            Subject.UpdateSeries(_fakeSeries);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, seasonNumber, monitored), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, It.IsAny<int>(), It.IsAny<bool>()), Times.Once());
        }
    }
}
