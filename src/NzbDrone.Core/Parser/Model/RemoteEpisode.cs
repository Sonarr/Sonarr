using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteEpisode
    {
        public ReleaseInfo Release { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public SceneMapping SceneMapping { get; set; }
        public int MappedSeasonNumber { get; set; }

        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public bool EpisodeRequested { get; set; }
        public bool DownloadAllowed { get; set; }
        public TorrentSeedConfiguration SeedConfiguration { get; set; }
        public int PreferredWordScore { get; set; }

        public RemoteEpisode()
        {
            Episodes = new List<Episode>();
        }

        public bool IsRecentEpisode()
        {
            return Episodes.Any(e => e.AirDateUtc >= DateTime.UtcNow.Date.AddDays(-14));
        }

        public override string ToString()
        {
            return Release.Title;
        }
    }
}
