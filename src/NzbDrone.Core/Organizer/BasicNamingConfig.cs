namespace NzbDrone.Core.Organizer
{
    public class BasicNamingConfig
    {
        public bool IncludeSeriesTitle { get; set; }
        public bool IncludeEpisodeTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public bool ReplaceSpaces { get; set; }
        public string Separator { get; set; }
        public string NumberStyle { get; set; }
    }
}
