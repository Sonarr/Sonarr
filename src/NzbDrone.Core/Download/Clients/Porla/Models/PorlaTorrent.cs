using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public sealed class PorlaTorrent
    {
        [JsonProperty("info_hash", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(LibTorrentInfoHashConverter))]
        public LibTorrentInfoHash InfoHash { get; set; }

        public PorlaTorrent(string hash, string status)
        {
            InfoHash = new LibTorrentInfoHash(hash, status);
        }

        public object[] AsParam()
        {
            string[] ret = { InfoHash.Hash, null };
            return ret;
        }

        public object[] AsParams()
        {
            object[] ret = { "info_hash", AsParam() };
            return ret;
        }
    }
}
