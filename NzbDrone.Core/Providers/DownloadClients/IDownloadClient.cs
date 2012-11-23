using System.Linq;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DownloadClients
{
    public interface IDownloadClient
    {
        bool IsInQueue(EpisodeParseResult newParseResult);
        bool DownloadNzb(string url, string title, bool recentlyAired);
    }
}
