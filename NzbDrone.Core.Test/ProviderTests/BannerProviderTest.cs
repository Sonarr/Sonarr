using System;
using System.IO;
using System.Linq;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class BannerProviderTest : CoreTest
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            WithTempAsAppPath();

            _series = Builder<Series>.CreateNew()
                .With(s => s.Id = 12345)
                    .Build();

            var path = @"C:\Windows\Temp";

            Mocker.GetMock<DiskProvider>().Setup(s => s.CreateDirectory(path));
        }

        private void WithSuccessfulDownload()
        {
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()));
        }

        private void WithFailedDownload()
        {
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new WebException("Failed to download file (Mocked)"));
        }

        [Test]
        public void Download_should_return_true_when_banner_is_downloaded_successfully()
        {
            WithSuccessfulDownload();
            var result = Mocker.Resolve<BannerProvider>().Download(_series);
            result.Should().BeTrue();
        }

        [Test]
        public void Download_should_return_false_when_banner_download_fails()
        {
            WithFailedDownload();
            var result = Mocker.Resolve<BannerProvider>().Download(_series);
            result.Should().BeFalse();
        }

        [Test]
        public void Delete_should_delete_banner_file_when_it_exists()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.FileExists(It.IsAny<string>()))
                    .Returns(true);

            Mocker.GetMock<DiskProvider>().Setup(s => s.DeleteFile(It.IsAny<string>()));

            var result = Mocker.Resolve<BannerProvider>().Delete(1);
            result.Should().BeTrue();
        }

        [Test]
        public void Delete_should_return_true_even_when_file_sint_deleted()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.FileExists(It.IsAny<string>()))
                    .Returns(false);

            var result = Mocker.Resolve<BannerProvider>().Delete(1);
            result.Should().BeTrue();
        }

        [Test]
        public void Delete_should_return_false_when_file_fails_to_delete()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.FileExists(It.IsAny<string>()))
                    .Returns(true);

            Mocker.GetMock<DiskProvider>().Setup(s => s.DeleteFile(It.IsAny<string>()))
                .Throws(new SystemException("File not found."));

            var result = Mocker.Resolve<BannerProvider>().Delete(1);
            result.Should().BeFalse();
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}