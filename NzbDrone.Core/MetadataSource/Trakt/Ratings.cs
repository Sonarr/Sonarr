namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class Ratings
    {
        public int percentage { get; set; }
        public int votes { get; set; }
        public int loved { get; set; }
        public int hated { get; set; }
    }
}