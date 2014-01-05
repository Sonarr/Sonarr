using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SpecialEpisodeSearchCriteria : SearchCriteriaBase
    {
        public string[] EpisodeQueryTitles { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            bool delimiter = false;
            foreach (var title in EpisodeQueryTitles)
            {
                if (delimiter)
                {
                    sb.Append(',');
                }
                sb.Append(title);
                delimiter = true;
            }
            return string.Format("[{0} : {1}]", SceneTitle, sb.ToString());
        }
    }
}
