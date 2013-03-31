namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class TopEpisode
    {
        public int plays { get; set; }
        public int season { get; set; }
        public int number { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public int first_aired { get; set; }
    }
}