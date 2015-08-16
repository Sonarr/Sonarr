using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.Series;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalEpisode : LocalItem
    {
        public LocalEpisode()
        {
            Episodes = new List<Episode>();
        }

        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        
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

        public override Media Media
        {
            get
            {
                return Series;
            }
        }

        public override IEnumerable<MediaModelBase> MediaFiles
        {
            get
            {
                return Episodes.Where(e => e.EpisodeFileId > 0).Select(e => e.EpisodeFile.Value);
            }
        }

        public ParsedEpisodeInfo ParsedEpisodeInfo
        {
            get
            {
                return ParsedInfo as ParsedEpisodeInfo;
            }

            set
            {
                ParsedInfo = value;
            }
        }

        public override bool IsEmpty()
        {
            return !Episodes.Any();
        }
    }
}