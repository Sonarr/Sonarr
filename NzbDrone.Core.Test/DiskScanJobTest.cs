// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class DiskScanJobTest : TestBase
    {
        [Test]
        public void series_specific_scan_should_scan_series()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12)
                .With(s => s.Episodes = Builder<Episode>.CreateListOfSize(10).Build())
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(series.SeriesId))
                .Returns(series);

            mocker.GetMock<MediaFileProvider>()
                .Setup(p => p.Scan(series))
                .Returns(new List<EpisodeFile>());


            //Act
            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), series.SeriesId);

            //Assert
            mocker.VerifyAllMocks();
        }


        [Test]
        public void series_with_no_episodes_should_skip_scan()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12)
                .With(s => s.Episodes = new List<Episode>())
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(series.SeriesId))
                .Returns(series);

            //Act
            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), series.SeriesId);

            //Assert
            mocker.VerifyAllMocks();
        }

        [Test]
        public void job_with_no_target_should_scan_all_series()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .WhereTheFirst(1).Has(s => s.SeriesId = 12)
                .AndTheNext(1).Has(s => s.SeriesId = 15)
                .WhereAll().Have(s => s.Episodes = Builder<Episode>.CreateListOfSize(10).Build())
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series.AsQueryable());

            mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.Scan(series[0]))
                .Returns(new List<EpisodeFile>());

            mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.Scan(series[1]))
                .Returns(new List<EpisodeFile>());

            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0);


            mocker.VerifyAllMocks();
        }

        [Test]
        public void failed_scan_should_not_terminated_job()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .WhereTheFirst(1).Has(s => s.SeriesId = 12)
                .AndTheNext(1).Has(s => s.SeriesId = 15)
                .WhereAll().Have(s => s.Episodes = Builder<Episode>.CreateListOfSize(10).Build())
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series.AsQueryable());

            mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.Scan(series[0]))
                .Throws(new InvalidOperationException("Bad Job"));

            mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.Scan(series[1]))
                .Throws(new InvalidOperationException("Bad Job"));

            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0);


            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedErrors(2);
        }

        [Test]
        public void job_with_no_target_should_scan_series_with_episodes()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .WhereTheFirst(1).Has(s => s.SeriesId = 12)
                .And(s => s.Episodes = Builder<Episode>.CreateListOfSize(10).Build())
                .AndTheNext(1).Has(s => s.SeriesId = 15)
                   .And(s => s.Episodes = new List<Episode>())
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series.AsQueryable());

            mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.Scan(series[0]))
                .Returns(new List<EpisodeFile>());

            mocker.Resolve<DiskScanJob>().Start(new ProgressNotification("Test"), 0);


            mocker.VerifyAllMocks();
        }
    }

}