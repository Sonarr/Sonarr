namespace NzbDrone.Core.Organizer
{
    public class EpisodeSortingType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string EpisodeSeparator { get; set; }
    }
}
