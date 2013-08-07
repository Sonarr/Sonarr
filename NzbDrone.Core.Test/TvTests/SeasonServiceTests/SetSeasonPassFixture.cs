using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.SeasonServiceTests
{
    [TestFixture]
    public class SetSeasonPassFixture : CoreTest<SeasonService>
    {
        private const Int32 SERIES_ID = 1;
        private List<Season> _seasons;
        
        [SetUp]
        public void Setup()
        {
            _seasons = Builder<Season>.CreateListOfSize(5)
                .All()
                .With(s => s.SeriesId = SERIES_ID)
                .Build()
                .ToList();

            Mocker.GetMock<ISeasonRepository>()
                  .Setup(s => s.GetSeasonBySeries(It.IsAny<Int32>()))
                  .Returns(_seasons);
        }

        [Test]
        public void should_updateMany()
        {
            Subject.SetSeasonPass(SERIES_ID, 1);

            Mocker.GetMock<ISeasonRepository>()
                .Verify(v => v.UpdateMany(It.IsAny<List<Season>>()), Times.Once());
        }

        [Test]
        public void should_set_lower_seasons_to_false()
        {
            const int seasonNumber = 3;

            var result = Subject.SetSeasonPass(SERIES_ID, seasonNumber);

            result.Where(s => s.SeasonNumber < seasonNumber).Should().OnlyContain(s => s.Monitored == false);
        }

        [Test]
        public void should_set_equal_or_higher_seasons_to_false()
        {
            const int seasonNumber = 3;

            var result = Subject.SetSeasonPass(SERIES_ID, seasonNumber);

            result.Where(s => s.SeasonNumber >= seasonNumber).Should().OnlyContain(s => s.Monitored == true);
        }

        [Test]
        public void should_set_episodes_in_lower_seasons_to_false()
        {
            const int seasonNumber = 3;

            Subject.SetSeasonPass(SERIES_ID, seasonNumber);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.SetEpisodeMonitoredBySeason(SERIES_ID, It.Is<Int32>(i => i < seasonNumber), false), Times.AtLeastOnce());

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.SetEpisodeMonitoredBySeason(SERIES_ID, It.Is<Int32>(i => i < seasonNumber), true), Times.Never());
        }

        [Test]
        public void should_set_episodes_in_equal_or_higher_seasons_to_false()
        {
            const int seasonNumber = 3;

            Subject.SetSeasonPass(SERIES_ID, seasonNumber);

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.SetEpisodeMonitoredBySeason(SERIES_ID, It.Is<Int32>(i => i >= seasonNumber), true), Times.AtLeastOnce());

            Mocker.GetMock<IEpisodeService>()
                .Verify(v => v.SetEpisodeMonitoredBySeason(SERIES_ID, It.Is<Int32>(i => i >= seasonNumber), false), Times.Never());
        }
    }
}
