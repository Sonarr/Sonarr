using NzbDrone.Api.REST;

namespace NzbDrone.Api.Naming
{
    public class NamingResource : RestResource
    {
        public bool RenameEpisodes { get; set; }
        public string Separator { get; set; }
        public int NumberStyle { get; set; }
        public bool IncludeSeriesTitle { get; set; }
        public bool IncludeEpisodeTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public bool ReplaceSpaces { get; set; }

        public string SingleEpisodeExample { get; set; }
        public string MultiEpisodeExample { get; set; }
    }
}