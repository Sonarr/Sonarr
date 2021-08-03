using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class RefreshSeriesServiceFixture : CoreTest<RefreshSeriesService>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            var season1 = Builder<Season>.CreateNew()
                                         .With(s => s.SeasonNumber = 1)
                                         .Build();

            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Status = SeriesStatusType.Continuing)
                                     .With(s => s.Seasons = new List<Season>
                                                            {
                                                                season1
                                                            })
                                     .Build();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(_series.Id))
                  .Returns(_series);

            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(It.IsAny<int>()))
                  .Callback<int>(p => { throw new SeriesNotFoundException(p); });
        }

        private void GivenNewSeriesInfo(Series series)
        {
            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(_series.TvdbId))
                  .Returns(new Tuple<Series, List<Episode>>(series, new List<Episode>()));
        }

        [Test]
        public void should_monitor_new_seasons_automatically_if_series_is_monitored()
        {
            _series.Monitored = true;
            var newSeriesInfo = _series.JsonClone();
            newSeriesInfo.Seasons.Add(Builder<Season>.CreateNew()
                                         .With(s => s.SeasonNumber = 2)
                                         .Build());

            GivenNewSeriesInfo(newSeriesInfo);

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Seasons.Count == 2 && s.Seasons.Single(season => season.SeasonNumber == 2).Monitored == true), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_monitor_new_seasons_automatically_if_series_is_not_monitored()
        {
            _series.Monitored = false;
            var newSeriesInfo = _series.JsonClone();
            newSeriesInfo.Seasons.Add(Builder<Season>.CreateNew()
                .With(s => s.SeasonNumber = 2)
                .Build());

            GivenNewSeriesInfo(newSeriesInfo);

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Seasons.Count == 2 && s.Seasons.Single(season => season.SeasonNumber == 2).Monitored == false), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_monitor_new_special_season_automatically()
        {
            var series = _series.JsonClone();
            series.Seasons.Add(Builder<Season>.CreateNew()
                                         .With(s => s.SeasonNumber = 0)
                                         .Build());

            GivenNewSeriesInfo(series);

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Seasons.Count == 2 && s.Seasons.Single(season => season.SeasonNumber == 0).Monitored == false), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_update_tvrage_id_if_changed()
        {
            var newSeriesInfo = _series.JsonClone();
            newSeriesInfo.TvRageId = _series.TvRageId + 1;

            GivenNewSeriesInfo(newSeriesInfo);

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.TvRageId == newSeriesInfo.TvRageId), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_update_tvmaze_id_if_changed()
        {
            var newSeriesInfo = _series.JsonClone();
            newSeriesInfo.TvMazeId = _series.TvMazeId + 1;

            GivenNewSeriesInfo(newSeriesInfo);

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.TvMazeId == newSeriesInfo.TvMazeId), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_log_error_if_tvdb_id_not_found()
        {
            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Status == SeriesStatusType.Deleted), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_mark_as_deleted_if_tvdb_id_not_found()
        {
            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Status == SeriesStatusType.Deleted), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_remark_as_deleted_if_tvdb_id_not_found()
        {
            _series.Status = SeriesStatusType.Deleted;

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.IsAny<Series>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_tvdb_id_changed()
        {
            var newSeriesInfo = _series.JsonClone();
            newSeriesInfo.TvdbId = _series.TvdbId + 1;

            GivenNewSeriesInfo(newSeriesInfo);

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.TvdbId == newSeriesInfo.TvdbId), It.IsAny<bool>(), It.IsAny<bool>()));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_throw_if_duplicate_season_is_in_existing_info()
        {
            var newSeriesInfo = _series.JsonClone();
            newSeriesInfo.Seasons.Add(Builder<Season>.CreateNew()
                                         .With(s => s.SeasonNumber = 2)
                                         .Build());

            _series.Seasons.Add(Builder<Season>.CreateNew()
                                         .With(s => s.SeasonNumber = 2)
                                         .Build());

            _series.Seasons.Add(Builder<Season>.CreateNew()
                                         .With(s => s.SeasonNumber = 2)
                                         .Build());

            GivenNewSeriesInfo(newSeriesInfo);

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Seasons.Count == 2), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_filter_duplicate_seasons()
        {
            var newSeriesInfo = _series.JsonClone();
            newSeriesInfo.Seasons.Add(Builder<Season>.CreateNew()
                                         .With(s => s.SeasonNumber = 2)
                                         .Build());

            newSeriesInfo.Seasons.Add(Builder<Season>.CreateNew()
                                         .With(s => s.SeasonNumber = 2)
                                         .Build());

            GivenNewSeriesInfo(newSeriesInfo);

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Seasons.Count == 2), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void should_rescan_series_if_updating_fails()
        {
            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(_series.Id))
                  .Throws(new IOException());

            Assert.Throws<IOException>(() => Subject.Execute(new RefreshSeriesCommand(_series.Id)));

            Mocker.GetMock<IDiskScanService>()
                  .Verify(v => v.Scan(_series), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_rescan_series_if_updating_fails_with_series_not_found()
        {
            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(_series.Id))
                  .Throws(new SeriesNotFoundException(_series.Id));

            Subject.Execute(new RefreshSeriesCommand(_series.Id));

            Mocker.GetMock<IDiskScanService>()
                  .Verify(v => v.Scan(_series), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
