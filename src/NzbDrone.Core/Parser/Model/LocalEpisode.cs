using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalEpisode
    {
        public String Path { get; set; }
        public Int64 Size { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public QualityModel Quality { get; set; }
        public Boolean ExistingFile { get; set; }
        
        public int SeasonNumber 
        { 
            get
            {
                return Episodes.Select(c => c.SeasonNumber).Distinct().Single();
            } 
        }
        
        public override string ToString()
        {
            return Path;
        }
    }
}