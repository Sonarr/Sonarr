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
    public class DiskScanJobTest : CoreTest
    {
        [Test]
        public void series_specific_scan_should_scan_series()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12)
                .Build();

            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(series.SeriesId))
                .Returns(series);

            Mocker.GetMock<DiskScanProvider>()
                .Setup(p => p.Scan(series))
                .Returns(new List<EpisodeFile>());


            //Act
            Mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), series.SeriesId, 0);

            //Assert
            Mocker.VerifyAllMocks();
        }



        [Test]
        public void job_with_no_target_should_scan_all_series()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .TheFirst(1).With(s => s.SeriesId = 12)
                .TheNext(1).With(s => s.SeriesId = 15)
                .Build();

            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);

            Mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[0]))
                .Returns(new List<EpisodeFile>());

            Mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[1]))
                .Returns(new List<EpisodeFile>());

            Mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0, 0);


            Mocker.VerifyAllMocks();
        }

        [Test]
        public void failed_scan_should_not_terminated_job()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .TheFirst(1).With(s => s.SeriesId = 12)
                .TheNext(1).With(s => s.SeriesId = 15)
                .Build();

            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);

            Mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[0]))
                .Throws(new InvalidOperationException("Bad Job"));

            Mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[1]))
                .Throws(new InvalidOperationException("Bad Job"));

            Mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0, 0);


            Mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedErrors(2);
        }

        [Test]
        public void job_with_no_target_should_scan_series_with_episodes()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .TheFirst(1).With(s => s.SeriesId = 12)
                .TheNext(1).With(s => s.SeriesId = 15)
                .Build();

            WithStrictMocker();

            Mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);

            Mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[0]))
                .Returns(new List<EpisodeFile>());

            Mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[1]))
                .Returns(new List<EpisodeFile>());

            Mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0, 0);



            Mocker.VerifyAllMocks();
            Mocker.GetMock<DiskScanProvider>().Verify(s => s.Scan(It.IsAny<Series>()), Times.Exactly(2));
        }
    }

}