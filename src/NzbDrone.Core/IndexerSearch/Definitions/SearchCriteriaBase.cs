using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public abstract class SearchCriteriaBase
    {
        private static readonly Regex SpecialCharacter = new Regex(@"[`'.]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex NonWord = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BeginningThe = new Regex(@"^the\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Media Media { get; set; }
        public List<String> SceneTitles { get; set; }
        public virtual bool MonitoredEpisodesOnly { get; set; }

        public List<String> QueryTitles
        {
            get
            {
                return SceneTitles.Select(GetQueryTitle).ToList();
            }
        }

        public static string GetQueryTitle(string title)
        {
            Ensure.That(title,() => title).IsNotNullOrWhiteSpace();

            var cleanTitle = BeginningThe.Replace(title, String.Empty);

            cleanTitle = cleanTitle.Replace("&", "and");
            cleanTitle = SpecialCharacter.Replace(cleanTitle, "");
            cleanTitle = NonWord.Replace(cleanTitle, "+");

            //remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{2,}", "+");
            cleanTitle = cleanTitle.RemoveAccent();
            return cleanTitle.Trim('+', ' ');
        }
    }

    public abstract class SeriesSearchCriteriaBase : SearchCriteriaBase
    {
        public List<Episode> Episodes { get; set; }
        public Series Series
        {
            get
            {
                return Media as Series;
            }
        }
    }

    public abstract class MovieSearchCriteriaBase : SearchCriteriaBase
    {
        public Movie Movie
        {
            get
            {
                return Media as Movie;
            }
        }
    }
}