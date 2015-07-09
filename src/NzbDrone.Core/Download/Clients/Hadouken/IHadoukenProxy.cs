using System.Collections.Generic;
using NzbDrone.Core.Download.Clients.Hadouken.Models;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public interface IHadoukenProxy
    {
        HadoukenSystemInfo GetSystemInfo(HadoukenSettings settings);
        IDictionary<string, HadoukenTorrent> GetTorrents(HadoukenSettings settings);
        IDictionary<string, object> GetConfig(HadoukenSettings settings);
        string AddTorrentFile(HadoukenSettings settings, byte[] fileContent);
        void AddTorrentUri(HadoukenSettings settings, string torrentUrl);
        void RemoveTorrent(HadoukenSettings settings, string downloadId, bool deleteData);
    }
}
