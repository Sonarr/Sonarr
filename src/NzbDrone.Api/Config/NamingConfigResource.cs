using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Config
{
    public class NamingConfigResource : RestResource
    {
        public Boolean RenameEpisodes { get; set; }
        public Int32 MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string DailyEpisodeFormat { get; set; }
        public string SeasonFolderFormat { get; set; }

        public bool IncludeSeriesTitle { get; set; }
        public bool IncludeEpisodeTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public bool ReplaceSpaces { get; set; }
        public string Separator { get; set; }
        public string NumberStyle { get; set; }
    }
}