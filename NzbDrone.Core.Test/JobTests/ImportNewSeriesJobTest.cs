// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{ 
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ImportNewSeriesJobTest : CoreTest
    {
        [Test]
        public void import_new_series_succesfull()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                     .All().With(s => s.LastInfoSync = null)
                     .TheFirst(1).With(s => s.SeriesId = 12)
                     .TheNext(1).With(s => s.SeriesId = 15)
                        .Build();

            var notification = new ProgressNotification("Test");

            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);


            Mocker.GetMock<DiskScanJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId, 0))
                .Callback(() => series[0].LastDiskSync = DateTime.Now);


            Mocker.GetMock<DiskScanJob>()
                .Setup(j => j.Start(notification, series[1].SeriesId, 0))
                .Callback(() => series[1].LastDiskSync = DateTime.Now);

            Mocker.GetMock<BannerDownloadJob>()
                .Setup(j => j.Start(notification, It.IsAny<int>(), 0));

            Mocker.GetMock<UpdateInfoJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId, 0))
                .Callback(() => series[0].LastInfoSync = DateTime.Now);

            Mocker.GetMock<UpdateInfoJob>()
                .Setup(j => j.Start(notification, series[1].SeriesId, 0))
                .Callback(() => series[1].LastInfoSync = DateTime.Now);

            Mocker.GetMock<SeriesProvider>()
                .Setup(s => s.GetSeries(series[0].SeriesId)).Returns(series[0]);

            Mocker.GetMock<SeriesProvider>()
                .Setup(s => s.GetSeries(series[1].SeriesId)).Returns(series[1]);

            Mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.GetSeriesFiles(It.IsAny<int>())).Returns(new List<EpisodeFile>());

            //Act
            Mocker.Resolve<ImportNewSeriesJob>().Start(notification, 0, 0);

            //Assert
            Mocker.VerifyAllMocks();

            Mocker.GetMock<DiskScanJob>().Verify(j => j.Start(notification, series[0].SeriesId, 0), Times.Once());
            Mocker.GetMock<DiskScanJob>().Verify(j => j.Start(notification, series[1].SeriesId, 0), Times.Once());

            Mocker.GetMock<UpdateInfoJob>().Verify(j => j.Start(notification, series[0].SeriesId, 0), Times.Once());
            Mocker.GetMock<UpdateInfoJob>().Verify(j => j.Start(notification, series[1].SeriesId, 0), Times.Once());

        }




        [Test]
        [Timeout(3000)]
        public void failed_import_should_not_be_stuck_in_loop()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                     .All().With(s => s.LastInfoSync = null)
                     .TheFirst(1).With(s => s.SeriesId = 12)
                     .TheNext(1).With(s => s.SeriesId = 15)
                        .Build();

            var notification = new ProgressNotification("Test");

            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);

            Mocker.GetMock<UpdateInfoJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId, 0))
                .Callback(() => series[0].LastInfoSync = DateTime.Now);

            Mocker.GetMock<UpdateInfoJob>()
                .Setup(j => j.Start(notification, series[1].SeriesId, 0))
                .Throws(new InvalidOperationException());

            Mocker.GetMock<DiskScanJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId, 0))
                .Callback(() => series[0].LastDiskSync = DateTime.Now);

            Mocker.GetMock<BannerDownloadJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId, 0));

            Mocker.GetMock<SeriesProvider>()
                .Setup(s => s.GetSeries(series[0].SeriesId)).Returns(series[0]);

            Mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.GetSeriesFiles(It.IsAny<int>())).Returns(new List<EpisodeFile>());

            //Act
            Mocker.Resolve<ImportNewSeriesJob>().Start(notification, 0, 0);

            //Assert
            Mocker.VerifyAllMocks();

            Mocker.GetMock<UpdateInfoJob>().Verify(j => j.Start(notification, series[0].SeriesId, 0), Times.Once());
            Mocker.GetMock<UpdateInfoJob>().Verify(j => j.Start(notification, series[1].SeriesId, 0), Times.Once());

            Mocker.GetMock<DiskScanJob>().Verify(j => j.Start(notification, series[0].SeriesId, 0), Times.Once());

            ExceptionVerification.ExpectedErrors(1);

        }



        [Test]
        public void AutoIgnoreSeason_new_series_should_not_ignore_any()
        {
            int seriesId = 12;

            WithStrictMocker();
            Mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.GetSeriesFiles(seriesId))
                .Returns(new List<EpisodeFile>());

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetSeasons(seriesId))
                .Returns(new List<int> { 0, 1, 2, 3, 4 });

            Mocker.Resolve<ImportNewSeriesJob>().AutoIgnoreSeasons(seriesId);


            Mocker.GetMock<EpisodeProvider>().Verify(p => p.SetSeasonIgnore(seriesId, It.IsAny<int>(), It.IsAny<Boolean>()), Times.Never());
        }

        [Test]
        public void AutoIgnoreSeason_existing_should_not_ignore_currentseason()
        {
            int seriesId = 12;

            var episodesFiles = Builder<EpisodeFile>.CreateListOfSize(2)
            .All().With(e => e.SeriesId = seriesId)
            .Build();

            episodesFiles[0].SeasonNumber = 0;
            episodesFiles[1].SeasonNumber = 1;

            WithStrictMocker();

            Mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.GetSeriesFiles(seriesId))
                .Returns(episodesFiles);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetSeasons(seriesId))
                .Returns(new List<int> { 0, 1, 2 });

            Mocker.Resolve<ImportNewSeriesJob>().AutoIgnoreSeasons(seriesId);

            Mocker.GetMock<EpisodeProvider>().Verify(p => p.SetSeasonIgnore(seriesId, 2, It.IsAny<Boolean>()), Times.Never());
        }

        [Test]
        public void AutoIgnoreSeason_existing_should_ignore_seasons_with_no_file()
        {
            int seriesId = 12;

            var episodesFiles = Builder<EpisodeFile>.CreateListOfSize(2)
            .All().With(e => e.SeriesId = seriesId)
            .Build();

            episodesFiles[0].SeasonNumber = 1;

            

            Mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.GetSeriesFiles(seriesId))
                .Returns(episodesFiles);

            Mocker.GetMock<EpisodeProvider>()
                .Setup(p => p.GetSeasons(seriesId))
                .Returns(new List<int> { 0, 1, 2 });

            Mocker.Resolve<ImportNewSeriesJob>().AutoIgnoreSeasons(seriesId);

            Mocker.GetMock<EpisodeProvider>().Verify(p => p.SetSeasonIgnore(seriesId, 0, true), Times.Once());
            Mocker.GetMock<EpisodeProvider>().Verify(p => p.SetSeasonIgnore(seriesId, 1, true), Times.Never());
            Mocker.GetMock<EpisodeProvider>().Verify(p => p.SetSeasonIgnore(seriesId, 2, It.IsAny<Boolean>()), Times.Never());
        }
    }


}