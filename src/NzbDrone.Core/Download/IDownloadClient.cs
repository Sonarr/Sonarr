using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClient : IProvider
    {
        string DownloadNzb(RemoteEpisode remoteEpisode);
        IEnumerable<QueueItem> GetQueue();
        IEnumerable<HistoryItem> GetHistory(int start = 0, int limit = 0);
        void RemoveFromQueue(string id);
        void RemoveFromHistory(string id);
    }
}
