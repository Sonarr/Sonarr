using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteEpisode
    {
        public ReportInfo Report { get; set; }

        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }

        public Series Series { get; set; }

        public List<Episode> Episodes { get; set; }

        public bool IsRecentEpisode()
        {
            return Episodes.Any(e => e.AirDateUtc >= DateTime.Today.AddDays(-14));
        }
    }
}