// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
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

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(series.SeriesId))
                .Returns(series);

            mocker.GetMock<DiskScanProvider>()
                .Setup(p => p.Scan(series))
                .Returns(new List<EpisodeFile>());


            //Act
            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), series.SeriesId, 0);

            //Assert
            mocker.VerifyAllMocks();
        }



        [Test]
        public void job_with_no_target_should_scan_all_series()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .TheFirst(1).With(s => s.SeriesId = 12)
                .TheNext(1).With(s => s.SeriesId = 15)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);

            mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[0]))
                .Returns(new List<EpisodeFile>());

            mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[1]))
                .Returns(new List<EpisodeFile>());

            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0, 0);


            mocker.VerifyAllMocks();
        }

        [Test]
        public void failed_scan_should_not_terminated_job()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .TheFirst(1).With(s => s.SeriesId = 12)
                .TheNext(1).With(s => s.SeriesId = 15)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);

            mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[0]))
                .Throws(new InvalidOperationException("Bad Job"));

            mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[1]))
                .Throws(new InvalidOperationException("Bad Job"));

            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0, 0);


            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedErrors(2);
        }

        [Test]
        public void job_with_no_target_should_scan_series_with_episodes()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .TheFirst(1).With(s => s.SeriesId = 12)
                .TheNext(1).With(s => s.SeriesId = 15)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);

            mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[0]))
                .Returns(new List<EpisodeFile>());

            mocker.GetMock<DiskScanProvider>()
                .Setup(s => s.Scan(series[1]))
                .Returns(new List<EpisodeFile>());

            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0, 0);



            mocker.VerifyAllMocks();
            mocker.GetMock<DiskScanProvider>().Verify(s => s.Scan(It.IsAny<Series>()), Times.Exactly(2));
        }
    }

}