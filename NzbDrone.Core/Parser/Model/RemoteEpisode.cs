using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteEpisode
    {
        public ReleaseInfo Release { get; set; }

        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }

        public Series Series { get; set; }

        public List<Episode> Episodes { get; set; }

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