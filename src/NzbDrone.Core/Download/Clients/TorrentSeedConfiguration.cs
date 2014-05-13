using System;

namespace NzbDrone.Core.Download.Clients
{
    public class TorrentSeedConfiguration
    {
        public static TorrentSeedConfiguration DefaultConfiguration = new TorrentSeedConfiguration();

        public Double? Ratio { get; set; }
        public TimeSpan? SeedTime { get; set; }
    }
}
