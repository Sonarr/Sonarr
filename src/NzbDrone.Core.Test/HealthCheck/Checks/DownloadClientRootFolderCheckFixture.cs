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
    public class DownloadClientRootFolderCheckFixture : CoreTest<DownloadClientRootFolderCheck>
    {
        private readonly string _downloadRootPath = @"c:\Test".AsOsAgnostic();

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
                OutputRootFolders = new List<OsPath> { new OsPath(_downloadRootPath) }
            };

            _downloadClient = Mocker.GetMock<IDownloadClient>();
            _downloadClient.Setup(s => s.Definition)
                .Returns(new DownloadClientDefinition { Name = "Test" });

            _downloadClient.Setup(s => s.GetStatus())
                .Returns(_clientStatus);

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClients())
                  .Returns(new IDownloadClient[] { _downloadClient.Object });

            Mocker.GetMock<IDiskProvider>()
                .Setup(x => x.FolderExists(It.IsAny<string>()))
                .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(x => x.FolderWritable(It.IsAny<string>()))
                  .Returns(true);
        }

        private void GivenRootFolder(string folder)
        {
            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.All())
                  .Returns(new List<RootFolder> { new RootFolder { Path = folder.AsOsAgnostic() } });
        }

        [Test]
        public void should_return_downloads_in_root_folder_if_downloading_to_root_folder()
        {
            GivenRootFolder(_downloadRootPath);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_ok_if_not_downloading_to_root_folder()
        {
            string rootFolderPath = "c:\\Test2".AsOsAgnostic();

            GivenRootFolder(rootFolderPath);

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
