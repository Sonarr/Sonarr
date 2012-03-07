using System.IO;
using System.Net;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class BannerDownloadJobTest : CoreTest
    {
        private ProgressNotification _notification;

        [SetUp]
        public void Setup()
        {
            _notification = new ProgressNotification("Test");
            WithTempAsAppPath();
        }

        private void WithSuccessfulDownload()
        {
            Mocker.GetMock<BannerProvider>()
                .Setup(s => s.Download(It.IsAny<Series>()))
                    .Returns(true);
        }

        private void WithFailedDownload()
        {
            Mocker.GetMock<BannerProvider>()
                .Setup(s => s.Download(It.IsAny<Series>()))
                    .Returns(false);
        }

        private void VerifyDownloadMock(int times)
        {
            Mocker.GetMock<BannerProvider>().Verify(v => v.Download(It.IsAny<Series>()), Times.Exactly(times));
        }

        [Test]
        public void Start_should_download_banners_for_all_series_when_no_targetId_is_passed_in()
        {
            WithSuccessfulDownload();

            var series = Builder<Series>.CreateListOfSize(5)
                    .Build();

            Mocker.GetMock<SeriesProvider>().Setup(s => s.GetAllSeries())
                    .Returns(series);

            Mocker.Resolve<BannerDownloadJob>().Start(_notification, 0, 0);
            VerifyDownloadMock(series.Count);
        }

        [Test]
        public void Start_should_only_attempt_to_download_for_series_with_banner_url()
        {
            WithSuccessfulDownload();

            var series = Builder<Series>.CreateListOfSize(5)
                    .TheFirst(2)
                    .With(s => s.BannerUrl = null)
                    .Build();

            Mocker.GetMock<SeriesProvider>().Setup(s => s.GetAllSeries())
                    .Returns(series);

            Mocker.Resolve<BannerDownloadJob>().Start(_notification, 0, 0);
            VerifyDownloadMock(3);
        }

        [Test]
        public void Start_should_download_single_banner_when_seriesId_is_passed_in()
        {
            WithSuccessfulDownload();

            var series = Builder<Series>.CreateNew()
                    .Build();

            Mocker.GetMock<SeriesProvider>().Setup(s => s.GetSeries(series.SeriesId))
                    .Returns(series);

            Mocker.Resolve<BannerDownloadJob>().Start(_notification, 1, 0);
            VerifyDownloadMock(1);
        }
    }
}