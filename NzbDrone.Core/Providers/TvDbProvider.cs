using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
using TvdbLib;
using TvdbLib.Cache;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class TvDbProvider
    {
        private const string TVDB_APIKEY = "5D2D188E86E07F4F";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex CleanUpRegex = new Regex(@"((\s|^)the(\s|$))|((\s|^)and(\s|$))|[^a-z]",
                                                               RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly TvdbHandler _handler;

        public TvDbProvider()
        {
            _handler = new TvdbHandler(new XmlCacheProvider(CentralDispatch.AppPath + @"\cache\tvdb"), TVDB_APIKEY);
        }

        public virtual IList<TvdbSearchResult> SearchSeries(string title)
        {
            lock (_handler)
            {
                Logger.Debug("Searching TVDB for '{0}'", title);

                var result = _handler.SearchSeries(title);

                Logger.Debug("Search for '{0}' returned {1} possible results", title, result.Count);
                return result;
            }
        }


        public virtual TvdbSearchResult GetSeries(string title)
        {
            lock (_handler)
            {
                var searchResults = SearchSeries(title);
                if (searchResults.Count == 0)
                    return null;

                foreach (var tvdbSearchResult in searchResults)
                {
                    if (IsTitleMatch(tvdbSearchResult.SeriesName, title))
                    {
                        Logger.Debug("Search for '{0}' was successful", title);
                        return tvdbSearchResult;
                    }
                }
            }
            return null;
        }

        public virtual int GetBestMatch(List<TvdbSearchResult> searchResults, string title)
        {
            if (searchResults.Count == 0)
                return 0;

            foreach (var tvdbSearchResult in searchResults)
            {
                if (IsTitleMatch(tvdbSearchResult.SeriesName, title))
                {
                    Logger.Debug("Search for '{0}' was successful", title);
                    return tvdbSearchResult.Id;
                }
            }

            return searchResults[0].Id;
        }

        public virtual TvdbSeries GetSeries(int id, bool loadEpisodes)
        {
            lock (_handler)
            {
                Logger.Debug("Fetching SeriesId'{0}' from tvdb", id);
                return _handler.GetSeries(id, TvdbLanguage.DefaultLanguage, loadEpisodes, false, false);
            }
        }

        /// <summary>
        ///   Determines whether a title in a search result is equal to the title searched for.
        /// </summary>
        /// <param name = "directoryName">Name of the directory.</param>
        /// <param name = "tvdbTitle">The TVDB title.</param>
        /// <returns>
        ///   <c>true</c> if the titles are found to be same; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTitleMatch(string directoryName, string tvdbTitle)
        {
            var result = false;

            if (String.IsNullOrEmpty(directoryName))
                throw new ArgumentException("directoryName");
            if (String.IsNullOrEmpty(tvdbTitle))
                throw new ArgumentException("tvdbTitle");

            if (String.Equals(directoryName, tvdbTitle, StringComparison.CurrentCultureIgnoreCase))
            {
                result = true;
            }
            else if (String.Equals(CleanUpRegex.Replace(directoryName, ""), CleanUpRegex.Replace(tvdbTitle, ""),
                                   StringComparison.InvariantCultureIgnoreCase))
                result = true;

            Logger.Debug("Match between '{0}' and '{1}' was {2}", tvdbTitle, directoryName, result);

            return result;
        }
    }
}