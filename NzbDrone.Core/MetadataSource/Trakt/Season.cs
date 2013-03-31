using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class Season
    {
        public int season { get; set; }
        public List<Episode> episodes { get; set; }
        public string url { get; set; }
        public string poster { get; set; }
    }
}