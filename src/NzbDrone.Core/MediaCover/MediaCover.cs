using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaCover
{

    public enum MediaCoverTypes
    {
        Unknown = 0,
        Poster = 1,
        Banner = 2,
        Fanart = 3,
        Screenshot = 4,
        Headshot = 5
    }

    public enum MediaCoverOrigin
    {
        Series = 0,
        Movie = 1
    }

    public class MediaCover : IEmbeddedDocument
    {
        public MediaCoverTypes CoverType { get; set; }
        public string Url { get; set; }
        public MediaCoverOrigin CoverOrigin { get; set; }

        public MediaCover()
        {
        }

        public MediaCover(MediaCoverTypes coverType, string url, MediaCoverOrigin coverOrigin)
        {
            CoverType = coverType;
            CoverOrigin = coverOrigin;
            Url = url;
        }
    }
}