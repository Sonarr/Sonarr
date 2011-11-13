using System.IO;
using System.Net;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class BannerDownloadJobTest : CoreTest
    {

        [SetUp]
        public void Setup()
        {
            WithStrictMocker();
            WithTempAsAppPath();
        }

        [Test]
        public void BannerDownload_all()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .Build();

            var notification = new ProgressNotification("Banner Download");

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetAllSeries())
                .Returns(fakeSeries);

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()));

            Mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            Mocker.Resolve<BannerDownloadJob>().Start(notification, 0, 0);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Exactly(fakeSeries.Count));
        }

        [Test]
        public void BannerDownload_some_null_BannerUrl()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .Random(2)
                .With(s => s.BannerUrl = null)
                .Build();

          var notification = new ProgressNotification("Banner Download");

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetAllSeries())
                .Returns(fakeSeries);

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()));

            Mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            Mocker.Resolve<BannerDownloadJob>().Start(notification, 0, 0);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Exactly(8));
        }

        [Test]
        public void BannerDownload_some_failed_download()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateListOfSize(4)
                .Build();


            var bannerPath = Mocker.GetMock<EnviromentProvider>().Object.GetBannerPath();

            var notification = new ProgressNotification("Banner Download");

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetAllSeries())
                .Returns(fakeSeries);

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(bannerPath, "1.jpg")))
                .Throws(new WebException());

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(bannerPath, "2.jpg")));

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(bannerPath, "3.jpg")))
                .Throws(new WebException());

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(bannerPath, "4.jpg")));

            Mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            Mocker.Resolve<BannerDownloadJob>().Start(notification, 0, 0);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Exactly(fakeSeries.Count));
        }

        [Test]
        public void BannerDownload_all_failed_download()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .Build();

            var notification = new ProgressNotification("Banner Download");

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetAllSeries())
                .Returns(fakeSeries);

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebException());

            Mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            Mocker.Resolve<BannerDownloadJob>().Start(notification, 0, 0);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Exactly(fakeSeries.Count));
        }

        [Test]
        public void BannerDownload_single_banner()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .Build();

            var notification = new ProgressNotification("Banner Download");

            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(1))
                .Returns(fakeSeries);

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebException());

            Mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            Mocker.Resolve<BannerDownloadJob>().Start(notification, 1, 0);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Once());
        }

        [Test]
        public void Download_Banner()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .Build();

            var notification = new ProgressNotification("Banner Download");

            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebException());

            //Act
            Mocker.Resolve<BannerDownloadJob>().DownloadBanner(notification, fakeSeries);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Once());
        }
    }
}