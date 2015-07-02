using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public abstract class SearchCriteriaBase
    {
        private static readonly Regex SpecialCharacter = new Regex(@"[`']", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex NonWordLessDot = new Regex(@"(?!\.)[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BeginningThe = new Regex(@"^the\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Series Series { get; set; }
        public List<String> SceneTitles { get; set; }
        public List<Episode> Episodes { get; set; }
        public virtual bool MonitoredEpisodesOnly { get; set; }

        public List<String> QueryTitles
        {
            get
            {
                return SceneTitles.SelectMany(GetQueryTitle).ToList();
            }
        }

        public static IEnumerable<string> GetQueryTitle(string title)
        {

            var cleanTitle = BeginningThe.Replace(title, String.Empty);

            cleanTitle = cleanTitle.Replace("&", "and");
            cleanTitle = SpecialCharacter.Replace(cleanTitle, "");
            cleanTitle = NonWordLessDot.Replace(cleanTitle, "+");

            // Remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{2,}", "+");
            cleanTitle = cleanTitle.RemoveAccent();
            cleanTitle = cleanTitle.Trim('+', ' ');

            if (!string.IsNullOrWhiteSpace(cleanTitle))
            {
                yield return cleanTitle;
            }

            // If the title contains a dot then add an additional title without it
            // as some series drop them.
            if (cleanTitle.IndexOf('.') > -1)
            {
                cleanTitle = cleanTitle.Replace(".", string.Empty);
                if (!string.IsNullOrWhiteSpace(cleanTitle))
                {
                    yield return cleanTitle;
                }
            }
        }
    }
}