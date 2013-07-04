using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteEpisode
    {
        public ReportInfo Report { get; set; }

        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }

        public Series Series { get; set; }

        public List<Episode> Episodes { get; set; }
    }
}