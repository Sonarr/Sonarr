using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalEpisode
    {
        public LocalEpisode()
        {
            Episodes = new List<Episode>();
        }

        public string Path { get; set; }
        public long Size { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }
        
        public int SeasonNumber 
        { 
            get
            {
                return Episodes.Select(c => c.SeasonNumber).Distinct().Single();
            } 
        }

        public bool IsSpecial => SeasonNumber == 0;

        public override string ToString()
        {
            return Path;
        }
    }
}