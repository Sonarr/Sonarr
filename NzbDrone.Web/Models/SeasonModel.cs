using System.Collections.Generic;

namespace NzbDrone.Web.Models
{
    public class SeasonModel
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public List<EpisodeModel> Episodes { get; set; }
        public bool AnyWanted { get; set; }
        public string CommonStatus { get; set; }
        public bool Ignored { get; set; }
    }
}