using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DownloadClientFolderCheckFixture : CoreTest<DownloadClientSortingCheck>
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
                SortingMode = null
            };

            _downloadClient = Mocker.GetMock<IDownloadClient>();
            _downloadClient.Setup(s => s.Definition)
                .Returns(new DownloadClientDefinition { Name = "Test" });

            _downloadClient.Setup(s => s.GetStatus())
                .Returns(_clientStatus);

            Mocker.GetMock<IProvideDownloadClient>()
                .Setup(s => s.GetDownloadClients())
                .Returns(new IDownloadClient[] { _downloadClient.Object });
        }

        [Test]
        public void should_return_ok_if_sorting_is_not_enabled()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_warning_if_sorting_is_enabled()
        {
            _clientStatus.SortingMode = "TV";

            Subject.Check().ShouldBeWarning();
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
