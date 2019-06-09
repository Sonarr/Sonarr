using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public class QBittorrentLabel
    {
        public string Name { get; set; }
        public string SavePath { get; set; }
    }
}
