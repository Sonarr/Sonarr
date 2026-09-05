using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class AnimeSeasonSearchCriteria : SearchCriteriaBase
    {
        public int SeasonNumber { get; set; }
        public List<string> SeasonSceneTitles { get; set; } = [];

        public List<string> AllSeasonSceneTitles => SeasonSceneTitles.Concat(CleanSeasonSceneTitles).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
        public List<string> CleanSeasonSceneTitles => SeasonSceneTitles.Select(GetCleanSceneTitle).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();

        public override string ToString()
        {
            return $"[{Series.Title} : S{SeasonNumber:00}]";
        }
    }
}
