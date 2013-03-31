using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaCover
{

    public enum MediaCoverTypes
    {
        Poster = 0,
        Banner = 1,
        Fanart = 2
    }

    public class MediaCover : IEmbeddedDocument
    {
        public MediaCoverTypes CoverType { get; set; }
        public string Url { get; set; }
    }
}