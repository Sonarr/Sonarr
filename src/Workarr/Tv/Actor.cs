using Workarr.Datastore;

namespace Workarr.Tv
{
    public class Actor : IEmbeddedDocument
    {
        public Actor()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string Name { get; set; }
        public string Character { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }
}
