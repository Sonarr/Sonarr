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
                return string.Format("[{0}] Specials", Series.Title);
            }

            return string.Format("[{0} : {1}]", Series.Title, string.Join(",", EpisodeQueryTitles));
        }
    }
}
