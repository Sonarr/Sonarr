using System.Linq;

namespace NzbDrone.Core.MediaCover
{

    public enum MediaCoverTypes
    {
        Poster = 0,
        Banner = 1,
        Fanart = 2
    }

    public class MediaCover
    {
        public MediaCoverTypes CoverType { get; set; }
        public string Url { get; set; }
    }
}