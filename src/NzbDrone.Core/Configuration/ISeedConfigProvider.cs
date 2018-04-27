using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Configuration
{
    public interface ISeedConfigProvider
    {
        TorrentSeedConfiguration GetSeedConfiguration(ReleaseInfo release);
    }
}
