using NUnit.Framework;
using NzbDrone.Core.Download.Clients.DownloadStation;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.DownloadStationTests
{
    [TestFixture]
    public class DownloadStationUsenetClientFixture : DownloadClientFixtureBase<DownloadStationUsenetClient>
    {
        protected DownloadStationSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new DownloadStationSettings
            {
                Host = "localhost",
                Port = 5000,
                Username = "admin",
                Password = "password"
            };
        }
    }
}
