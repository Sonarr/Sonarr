using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DownloadClientRemovesCompletedDownloadsCheckFixture : CoreTest<DownloadClientRemovesCompletedDownloadsCheck>
    {
        private DownloadClientInfo _clientStatus;
        private Mock<IDownloadClient> _downloadClient;

        private static Exception[] DownloadClientExceptions =
        {
            new DownloadClientUnavailableException("error"),
            new DownloadClientAuthenticationException("error"),
            new DownloadClientException("error")
        };

        [SetUp]
        public void Setup()
        {
            _clientStatus = new DownloadClientInfo
            {
                IsLocalhost = true,
                SortingMode = null,
                RemovesCompletedDownloads = true
            };

            _downloadClient = Mocker.GetMock<IDownloadClient>();
            _downloadClient.Setup(s => s.Definition)
                .Returns(new DownloadClientDefinition { Name = "Test" });

            _downloadClient.Setup(s => s.GetStatus())
                .Returns(_clientStatus);

            Mocker.GetMock<IProvideDownloadClient>()
                .Setup(s => s.GetDownloadClients(It.IsAny<bool>()))
                .Returns(new IDownloadClient[] { _downloadClient.Object });

            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                .Returns("Some Warning Message");
        }

        [Test]
        public void should_return_warning_if_removing_completed_downloads_is_enabled()
        {
            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_ok_if_remove_completed_downloads_is_not_enabled()
        {
            _clientStatus.RemovesCompletedDownloads = false;
            Subject.Check().ShouldBeOk();
        }

        [Test]
        [TestCaseSource("DownloadClientExceptions")]
        public void should_return_ok_if_client_throws_downloadclientexception(Exception ex)
        {
            _downloadClient.Setup(s => s.GetStatus())
                .Throws(ex);

            Subject.Check().ShouldBeOk();

            ExceptionVerification.ExpectedErrors(0);
        }
    }
}
