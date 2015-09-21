using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteEpisode : RemoteItem
    {
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo
        {
            get
            {
                return ParsedInfo as ParsedEpisodeInfo;
            }

            set
            {
                this.ParsedInfo = value;
            }
        }

        public bool IsRecentEpisode()
        {
            return Episodes.Any(e => e.AirDateUtc >= DateTime.UtcNow.Date.AddDays(-14));
        }

        public override Media Media
        {
            get
            {
                return Series;
            }
        }

        public override IEnumerable<Datastore.MediaModelBase> MediaFiles
        {
            get
            {
                return Episodes.Where(e => e.EpisodeFileId > 0).Select(e => e.EpisodeFile.Value);
            }
        }
    }
}