using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.SeasonServiceTests
{
    [TestFixture]
    public class SetMonitoredFixture : CoreTest<SeasonService>
    {
        private Season _season;
        
        [SetUp]
        public void Setup()
        {
            _season = new Season
            {
                Id = 1,
                SeasonNumber = 1,
                SeriesId = 1,
                Monitored = false
            };

            Mocker.GetMock<ISeasonRepository>()
                  .Setup(s => s.Get(It.IsAny<Int32>(), It.IsAny<Int32>()))
                  .Returns(_season);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void should_update_season(bool monitored)
        {
            Subject.SetMonitored(_season.SeriesId, _season.SeasonNumber, monitored);

            Mocker.GetMock<ISeasonRepository>()
                .Verify(v => v.Update(_season), Times.Once());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void should_update_episodes_in_season(bool monitored)
        {
            Subject.SetMonitored(_season.SeriesId, _season.SeasonNumber, monitored);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.SetEpisodeMonitoredBySeason(_season.SeriesId, _season.SeasonNumber, monitored), Times.Once());
        }
    }
}
