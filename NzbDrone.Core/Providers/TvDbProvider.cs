using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using TvdbLib;
using TvdbLib.Cache;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class TvDbProvider : ITvDbProvider
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly Regex CleanUpRegex = new Regex(@"((\s|^)the(\s|$))|((\s|^)and(\s|$))|[^a-z]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string TVDB_APIKEY = "5D2D188E86E07F4F";
        private readonly TvdbHandler _handler;

        public TvDbProvider()
        {
            _handler = new TvdbHandler(new XmlCacheProvider(CentralDispatch.AppPath + @"\cache\tvdb"), TVDB_APIKEY);
        }

        #region ITvDbProvider Members

        public IList<TvdbSearchResult> SearchSeries(string title)
        {
            Logger.Debug("Searching TVDB for '{0}'", title);
            var result = new List<TvdbSearchResult>();

            foreach (var tvdbSearchResult in _handler.SearchSeries(title))
            {
                if (IsTitleMatch(tvdbSearchResult.SeriesName, title))
                {
                    result.Add(tvdbSearchResult);
                }
            }

            Logger.Debug("Search for '{0}' returned {1} results", title, result.Count);
            return result;
        }


        public TvdbSearchResult GetSeries(string title)
        {
            var searchResults = SearchSeries(title);
            if (searchResults.Count == 0)
                return null;

            return searchResults[0];
        }

        public TvdbSeries GetSeries(int id, bool loadEpisodes)
        {
            Logger.Debug("Fetching SeriesId'{0}' from tvdb", id);
            return _handler.GetSeries(id, TvdbLanguage.DefaultLanguage, loadEpisodes, false, false);
        }

        /// <summary>
        /// Determines whether a title in a search result is equal to the title searched for.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="tvdbTitle">The TVDB title.</param>
        /// <returns>
        /// 	<c>true</c> if the titles are found to be same; otherwise, <c>false</c>.
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
            else if (String.Equals(CleanUpRegex.Replace(directoryName, ""), CleanUpRegex.Replace(tvdbTitle, ""), StringComparison.InvariantCultureIgnoreCase))
                result = true;

            Logger.Debug("Match between '{0}' and '{1}' was {2}", tvdbTitle, directoryName, result);

            return result;
        }

        #endregion
    }
}