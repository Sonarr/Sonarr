using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.TorrentClientBaseTests
{
    public class TestTorrentClientSettings : DownloadClientSettingsBase<TestTorrentClientSettings>
    {
        public override NzbDroneValidationResult Validate() => new();
    }
}
