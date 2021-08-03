using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public class SearchSeriesComparer : IComparer<Series>
    {
        private static readonly Regex RegexCleanPunctuation = new Regex("[-._:]", RegexOptions.Compiled);
        private static readonly Regex RegexCleanCountryYearPostfix = new Regex(@"(?<=.+)( \([A-Z]{2}\)| \(\d{4}\)| \([A-Z]{2}\) \(\d{4}\))$", RegexOptions.Compiled);
        private static readonly Regex ArticleRegex = new Regex(@"^(a|an|the)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string SearchQuery { get; private set; }

        private readonly string _searchQueryWithoutYear;
        private int? _year;

        public SearchSeriesComparer(string searchQuery)
        {
            SearchQuery = searchQuery;

            var match = Regex.Match(SearchQuery, @"^(?<query>.+)\s+(?:\((?<year>\d{4})\)|(?<year>\d{4}))$");
            if (match.Success)
            {
                _searchQueryWithoutYear = match.Groups["query"].Value.ToLowerInvariant();
                _year = int.Parse(match.Groups["year"].Value);
            }
            else
            {
                _searchQueryWithoutYear = searchQuery.ToLowerInvariant();
            }
        }

        public int Compare(Series x, Series y)
        {
            int result = 0;

            // Prefer exact matches
            result = Compare(x, y, s => CleanPunctuation(s.Title).Equals(CleanPunctuation(SearchQuery)));
            if (result != 0)
            {
                return -result;
            }

            // Remove Articles (a/an/the)
            result = Compare(x, y, s => CleanArticles(s.Title).Equals(CleanArticles(SearchQuery)));
            if (result != 0)
            {
                return -result;
            }

            // Prefer close matches
            result = Compare(x, y, s => CleanPunctuation(s.Title).LevenshteinDistance(CleanPunctuation(SearchQuery)) <= 1);
            if (result != 0)
            {
                return -result;
            }

            // Compare clean matches by year "Battlestar Galactica 1978"
            result = CompareWithYear(x, y, s => CleanTitle(s.Title).LevenshteinDistance(_searchQueryWithoutYear) <= 1);
            if (result != 0)
            {
                return -result;
            }

            // Compare prefix matches by year "(CSI: ..."
            result = CompareWithYear(x, y, s => s.Title.ToLowerInvariant().StartsWith(_searchQueryWithoutYear + ":"));
            if (result != 0)
            {
                return -result;
            }

            return Compare(x, y, s => SearchQuery.LevenshteinDistanceClean(s.Title) - GetYearFactor(s));
        }

        public int Compare<T>(Series x, Series y, Func<Series, T> keySelector)
            where T : IComparable<T>
        {
            var keyX = keySelector(x);
            var keyY = keySelector(y);

            return keyX.CompareTo(keyY);
        }

        public int CompareWithYear(Series x, Series y, Predicate<Series> canMatch)
        {
            var matchX = canMatch(x);
            var matchY = canMatch(y);

            if (matchX && matchY)
            {
                if (_year.HasValue)
                {
                    var result = Compare(x, y, s => s.Year == _year.Value);
                    if (result != 0)
                    {
                        return result;
                    }
                }

                return Compare(x, y, s => s.Year);
            }

            return matchX.CompareTo(matchY);
        }

        private string CleanPunctuation(string title)
        {
            title = RegexCleanPunctuation.Replace(title, "");

            return title.ToLowerInvariant();
        }

        private string CleanTitle(string title)
        {
            title = RegexCleanPunctuation.Replace(title, "");
            title = RegexCleanCountryYearPostfix.Replace(title, "");

            return title.ToLowerInvariant();
        }

        private string CleanArticles(string title)
        {
            title = ArticleRegex.Replace(title, "");

            return title.Trim().ToLowerInvariant();
        }

        private int GetYearFactor(Series series)
        {
            if (_year.HasValue)
            {
                var offset = Math.Abs(series.Year - _year.Value);
                if (offset <= 1)
                {
                    return 20 - (10 * offset);
                }
            }

            return 0;
        }
    }
}
