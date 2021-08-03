using System;
using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DownloadClientCheckFixture : CoreTest<DownloadClientCheck>
    {
        [Test]
        public void should_return_warning_when_download_client_has_not_been_configured()
        {
            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClients())
                  .Returns(new IDownloadClient[0]);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_error_when_download_client_throws()
        {
            var downloadClient = Mocker.GetMock<IDownloadClient>();
            downloadClient.Setup(s => s.Definition).Returns(new DownloadClientDefinition { Name = "Test" });

            downloadClient.Setup(s => s.GetItems())
                          .Throws<Exception>();

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClients())
                  .Returns(new IDownloadClient[] { downloadClient.Object });

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_ok_when_download_client_returns()
        {
            var downloadClient = Mocker.GetMock<IDownloadClient>();

            downloadClient.Setup(s => s.GetItems())
                          .Returns(new List<DownloadClientItem>());

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClients())
                  .Returns(new IDownloadClient[] { downloadClient.Object });

            Subject.Check().ShouldBeOk();
        }
    }
}
