using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Config
{
    public class NamingConfigResource : RestResource
    {
        public Boolean IncludeEpisodeTitle { get; set; }
        public Boolean ReplaceSpaces { get; set; }
        public Boolean RenameEpisodes { get; set; }
        public Int32 MultiEpisodeStyle { get; set; }
        public Int32 NumberStyle { get; set; }
        public String Separator { get; set; }
        public Boolean IncludeQuality { get; set; }
        public Boolean IncludeSeriesTitle { get; set; }
    }
}