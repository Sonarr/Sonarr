namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class Episode
    {
        public int season { get; set; }
        public int episode { get; set; }
        public int number { get; set; }
        public int tvdb_id { get; set; }
        public string title { get; set; }
        public string overview { get; set; }
        public int first_aired { get; set; }
        public string first_aired_iso { get; set; }
        public int first_aired_utc { get; set; }
        public string url { get; set; }
        public string screen { get; set; }
        public Ratings ratings { get; set; }
        public Images images { get; set; }
    }
}