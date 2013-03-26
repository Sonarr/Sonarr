using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaCover
{

    public enum MediaCoverTypes
    {
        Poster = 0,
        Banner = 1,
        Fanart = 2
    }

    public class MediaCover : ModelBase
    {
        public MediaCoverTypes CoverType { get; set; }
        public string Url { get; set; }
        public int SeriesId { get; set; }
    }
}