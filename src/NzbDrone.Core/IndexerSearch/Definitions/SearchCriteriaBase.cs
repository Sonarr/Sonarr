using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public abstract class SearchCriteriaBase
    {
        private static readonly Regex NonWord = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BeginningThe = new Regex(@"^the\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Series Series { get; set; }
        public string SceneTitle { get; set; }
        public List<Episode> Episodes { get; set; }
        // if this is true, then the indexer can use a text query to search for missing episodes or special named episodes
        public bool UseIndexerTextSearch { get; set; }

        public string QueryTitle
        {
            get
            {
                return GetQueryTitle(SceneTitle);
            }
        }

        private static string GetQueryTitle(string title)
        {
            Ensure.That(title,() => title).IsNotNullOrWhiteSpace();

            var cleanTitle = BeginningThe.Replace(title, String.Empty);

            cleanTitle = cleanTitle
                .Replace("&", "and")
                .Replace("`", "")
                .Replace("'", "");

            cleanTitle = NonWord.Replace(cleanTitle, "+");

            //remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{2,}", "+");
            return cleanTitle.Trim('+', ' ');
        }
    }
}