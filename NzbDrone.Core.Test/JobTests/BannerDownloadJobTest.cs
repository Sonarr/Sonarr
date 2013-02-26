using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using System.Linq;

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
                    .Build().ToList();

            Mocker.GetMock<ISeriesRepository>().Setup(s => s.All())
                    .Returns(series);

            Mocker.Resolve<BannerDownloadJob>().Start(_notification, null);
            VerifyDownloadMock(series.Count);
        }

        [Test]
        public void Start_should_only_attempt_to_download_for_series_with_banner_url()
        {
            WithSuccessfulDownload();

            var series = Builder<Series>.CreateListOfSize(5)
                    .TheFirst(2)
                    .With(s => s.BannerUrl = null)
                    .Build().ToList();

            Mocker.GetMock<ISeriesRepository>().Setup(s => s.All())
                    .Returns(series);

            Mocker.Resolve<BannerDownloadJob>().Start(_notification, null);
            VerifyDownloadMock(3);
        }

        [Test]
        public void Start_should_download_single_banner_when_seriesId_is_passed_in()
        {
            WithSuccessfulDownload();

            var series = Builder<Series>.CreateNew()
                    .Build();

            Mocker.GetMock<ISeriesRepository>().Setup(s => s.Get(series.Id))
                    .Returns(series);

            Mocker.Resolve<BannerDownloadJob>().Start(_notification, new { SeriesId = series.Id });
            VerifyDownloadMock(1);
        }
    }
}