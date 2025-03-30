using System.Linq;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SpecialEpisodeSearchCriteria : SearchCriteriaBase
    {
        public string[] EpisodeQueryTitles { get; set; }

        public override string ToString()
        {
            var episodeTitles = EpisodeQueryTitles.ToList();

            if (episodeTitles.Count > 0)
            {
                return $"[{Series.Title} ({Series.SeriesType})] Specials";
            }

            return $"[{Series.Title} ({Series.SeriesType}): {string.Join(",", EpisodeQueryTitles)}]";
        }
    }
}
