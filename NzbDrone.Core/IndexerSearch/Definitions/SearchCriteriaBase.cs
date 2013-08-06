using System;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public abstract class SearchCriteriaBase
    {
        private static readonly Regex NoneWord = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BeginningThe = new Regex(@"^the\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public int SeriesId { get; set; }
        public int SeriesTvRageId { get; set; }
        public string SceneTitle { get; set; }

        public string QueryTitle
        {
            get
            {
                return GetQueryTitle(SceneTitle);
            }
        }

        private static string GetQueryTitle(string title)
        {
            Ensure.That(() => title).IsNotNullOrWhiteSpace();

            var cleanTitle = BeginningThe.Replace(title, String.Empty);

            cleanTitle = cleanTitle
                .Replace("&", "and")
                .Replace("`", "")
                .Replace("'", "");

            cleanTitle = NoneWord.Replace(cleanTitle, "+");

            //remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{2,}", "+");
            return cleanTitle.Trim('+', ' ');
        }
    }
}