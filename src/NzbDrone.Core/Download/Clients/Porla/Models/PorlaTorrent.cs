using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    /// <summary> Wraps the LibTorrent Infohash type into a Porla Type for easier handling </summary>
    public sealed class PorlaTorrent
    {
        [JsonProperty("info_hash", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(LibTorrentInfoHashConverter))]
        public LibTorrentInfoHash InfoHash { get; set; }

        [JsonConstructor]
        public PorlaTorrent(string hash, string status)
        {
            InfoHash = new LibTorrentInfoHash(hash, status);
        }

        /// <summary> Converts a LibTorrentInfoHash into a PorlaTorrent </summary>
        public PorlaTorrent(LibTorrentInfoHash libTorrentInfoHash)
        {
            InfoHash = libTorrentInfoHash;
        }

        /// <summary> Converts a PorlaTorrent into an array of parameters </summary>
        public object[] AsParameters()
        {
            return new object[] { InfoHash.Hash, null };
        }

        /// <summary> Converts a PorlaTorrent into an array of parameters in the format [ "identifier", object, ...] </summary>
        public object[] AsQualifiedParameters()
        {
            return new object[] { "info_hash", AsParameters() };
        }
    }
}
