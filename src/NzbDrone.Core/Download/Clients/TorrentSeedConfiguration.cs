using System;

namespace NzbDrone.Core.Download.Clients
{
    public class TorrentSeedConfiguration
    {
        public static TorrentSeedConfiguration DefaultConfiguration = new ();

        public double? Ratio { get; set; }
        public TimeSpan? SeedTime { get; set; }
    }
}
