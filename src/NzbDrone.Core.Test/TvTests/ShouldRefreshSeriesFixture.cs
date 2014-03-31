using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class ShouldRefreshSeriesFixture : TestBase<ShouldRefreshSeries>
    {
        private Series _series;
        
        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .Build();
        }

        private void GivenSeriesIsEnded()
        {
            _series.Status = SeriesStatusType.Ended;
        }

        private void GivenSeriesLastRefreshedRecently()
        {
            _series.LastInfoSync = DateTime.UtcNow.AddDays(-1);
        }

        [Test]
        public void should_return_true_if_series_is_continuing()
        {
            _series.Status = SeriesStatusType.Continuing;

            Subject.ShouldRefresh(_series).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_series_last_refreshed_more_than_30_days_ago()
        {
            GivenSeriesIsEnded();
            _series.LastInfoSync = DateTime.UtcNow.AddDays(-100);

            Subject.ShouldRefresh(_series).Should().BeTrue();
        }

        [Test]
        public void should_should_return_true_if_episode_aired_in_last_30_days()
        {
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodeBySeries(_series.Id))
                  .Returns(Builder<Episode>.CreateListOfSize(2)
                                           .TheFirst(1)
                                           .With(e => e.AirDateUtc = DateTime.Today.AddDays(-7))
                                           .TheLast(1)
                                           .With(e => e.AirDateUtc = DateTime.Today.AddDays(-100))
                                           .Build()
                                           .ToList());

            Subject.ShouldRefresh(_series).Should().BeTrue();
        }

        [Test]
        public void should_should_return_false_when_recently_refreshed_ended_show_has_not_aired_for_30_days()
        {
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodeBySeries(_series.Id))
                  .Returns(Builder<Episode>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.AirDateUtc = DateTime.Today.AddDays(-100))
                                           .Build()
                                           .ToList());

            Subject.ShouldRefresh(_series).Should().BeTrue();
        }
    }
}
