using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClient
    {
        void DownloadNzb(RemoteEpisode remoteEpisode);
        bool IsConfigured { get; }
        IEnumerable<QueueItem> GetQueue();
    }

}
