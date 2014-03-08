using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DownloadClientCheckFixture : CoreTest<DownloadClientCheck>
    {
        [Test]
        public void should_return_warning_when_download_client_has_not_been_configured()
        {
            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClient())
                  .Returns((IDownloadClient)null);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_error_when_download_client_throws()
        {
            var downloadClient = Mocker.GetMock<IDownloadClient>();

            downloadClient.Setup(s => s.GetQueue())
                          .Throws<Exception>();

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClient())
                  .Returns(downloadClient.Object);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_null_when_download_client_returns()
        {
            var downloadClient = Mocker.GetMock<IDownloadClient>();

            downloadClient.Setup(s => s.GetQueue())
                          .Returns(new List<QueueItem>());

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(s => s.GetDownloadClient())
                  .Returns(downloadClient.Object);

            Subject.Check().Should().BeNull();
        }
    }
}
