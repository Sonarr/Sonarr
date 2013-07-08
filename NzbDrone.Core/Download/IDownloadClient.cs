using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClient
    {
        bool DownloadNzb(RemoteEpisode remoteEpisode);
        IEnumerable<QueueItem> GetQueue();
    }

}
