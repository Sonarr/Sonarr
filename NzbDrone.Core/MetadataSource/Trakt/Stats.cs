namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class Stats
    {
        public int watchers { get; set; }
        public int plays { get; set; }
        public int scrobbles { get; set; }
        public int scrobbles_unique { get; set; }
        public int checkins { get; set; }
        public int checkins_unique { get; set; }
        public int collection { get; set; }
        public int collection_unique { get; set; }
    }
}