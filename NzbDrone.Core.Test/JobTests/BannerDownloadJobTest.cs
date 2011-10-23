using System.IO;
using System.Net;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
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
    public class BannerDownloadJobTest : TestBase
    {
        [Test]
        public void BannerDownload_all()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.Resolve<EnviromentProvider>();

            var notification = new ProgressNotification("Banner Download");

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetAllSeries())
                .Returns(fakeSeries);

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()));

            mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            mocker.Resolve<BannerDownloadJob>().Start(notification, 0, 0);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
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

            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.Resolve<EnviromentProvider>();

            var notification = new ProgressNotification("Banner Download");

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetAllSeries())
                .Returns(fakeSeries);

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()));

            mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            mocker.Resolve<BannerDownloadJob>().Start(notification, 0, 0);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Exactly(8));
        }

        [Test]
        public void BannerDownload_some_failed_download()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .Build();

            var path = Path.Combine(new EnviromentProvider().AppPath, "Content", "Images", "Banners");

            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.Resolve<EnviromentProvider>();

            var notification = new ProgressNotification("Banner Download");

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetAllSeries())
                .Returns(fakeSeries);

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "1.jpg")))
                .Throws(new WebException());

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "2.jpg")));

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "3.jpg")))
                .Throws(new WebException());

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "4.jpg")));


            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "5.jpg")))
                .Throws(new WebException());

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "6.jpg")));


            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "7.jpg")))
                .Throws(new WebException());

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "8.jpg")));


            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "9.jpg")))
                .Throws(new WebException());

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), Path.Combine(path, "10.jpg")));


            mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            mocker.Resolve<BannerDownloadJob>().Start(notification, 0, 0);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Exactly(fakeSeries.Count));
        }

        [Test]
        public void BannerDownload_all_failed_download()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.Resolve<EnviromentProvider>();

            var notification = new ProgressNotification("Banner Download");

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetAllSeries())
                .Returns(fakeSeries);

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebException());

            mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            mocker.Resolve<BannerDownloadJob>().Start(notification, 0, 0);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Exactly(fakeSeries.Count));
        }

        [Test]
        public void BannerDownload_single_banner()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.Resolve<EnviromentProvider>();

            var notification = new ProgressNotification("Banner Download");

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(1))
                .Returns(fakeSeries);

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebException());

            mocker.GetMock<DiskProvider>()
                .Setup(S => S.CreateDirectory(It.IsAny<string>()))
                .Returns("");

            //Act
            mocker.Resolve<BannerDownloadJob>().Start(notification, 1, 0);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Once());
        }

        [Test]
        public void Download_Banner()
        {
            //Setup
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Banner Download");

            mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebException());

            //Act
            mocker.Resolve<BannerDownloadJob>().DownloadBanner(notification, fakeSeries);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<HttpProvider>().Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()),
                                                       Times.Once());
        }
    }
}