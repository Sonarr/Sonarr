using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClient
    {
        bool DownloadNzb(string url, string title);
        IEnumerable<QueueItem> GetQueue();
    }

}
