using System.Collections.Generic;
using NzbDrone.Core.Tv;
using System.Linq;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalEpisode
    {
        public string Path { get; set; }

        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }

        public Series Series { get; set; }

        public List<Episode> Episodes { get; set; }

        public QualityModel Quality { get; set; }

        public int SeasonNumber { get { return Episodes.Select(c => c.SeasonNumber).Distinct().Single(); } }

        public override string ToString()
        {
            return Path;
        }
    }
}