using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Profiles.Delay
{
    public class DelayProfile : ModelBase
    {
        public bool EnableUsenet { get; set; }
        public bool EnableTorrent { get; set; }
        public bool EnableFilehoster { get; set; }
        public DownloadProtocol PreferredProtocol { get; set; }
        public int UsenetDelay { get; set; }
        public int TorrentDelay { get; set; }
        public int FilehosterDelay { get; set; }
        public int Order { get; set; }
        public HashSet<int> Tags { get; set; }

        public DelayProfile()
        {
            Tags = new HashSet<int>();
        }

        public int GetProtocolDelay(DownloadProtocol protocol)
        {
            switch (protocol)
            {
                case DownloadProtocol.Unknown:
                case DownloadProtocol.Usenet:
                    return UsenetDelay;
                case DownloadProtocol.Torrent:
                    return TorrentDelay;
                case DownloadProtocol.Filehoster:
                    return FilehosterDelay;
                default:
                    throw new ArgumentOutOfRangeException("protocol", protocol, null);
            }
        }
    }
}
