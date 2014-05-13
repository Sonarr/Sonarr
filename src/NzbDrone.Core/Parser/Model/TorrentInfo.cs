using System;

namespace NzbDrone.Core.Parser.Model
{
    public class TorrentInfo : ReleaseInfo
    {
        public string MagnetUrl { get; set; }
        public string InfoHash { get; set; }
        public Int32? Seeds { get; set; }
        public Int32? Peers { get; set; }
    }
}