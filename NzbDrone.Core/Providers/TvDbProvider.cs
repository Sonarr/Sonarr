using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Ninject;
using NzbDrone.Common;
using XemLib;
using XemLib.Data;

namespace NzbDrone.Core.Providers
{
    public class TvDbProvider
    {
        private readonly EnvironmentProvider _environmentProvider;
        public const string TVDB_APIKEY = "5D2D188E86E07F4F";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly XemClient _xemClient;

        [Inject]
        public TvDbProvider(EnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
            _xemClient = new XemClient(TVDB_APIKEY);
        }

        public TvDbProvider()
        {

        }

        public virtual IList<TvdbSearchResult> SearchSeries(string title)
        {
            Logger.Debug("Searching TVDB for '{0}'", title);

            var result = _xemClient.SearchSeries(title);

            Logger.Debug("Search for '{0}' returned {1} possible results", title, result.Count);
            return result;
        }

        public virtual TvdbSeries GetSeries(int id, bool loadEpisodes, bool loadActors = false, bool loadBanners = false)
        {
            Logger.Debug("Fetching SeriesId'{0}' from tvdb", id);
            var result = _xemClient.GetSeries(id, loadEpisodes, loadActors, true, TvdbLanguage.Default);

            //Remove duplicated episodes
            var episodes = result.Episodes.OrderByDescending(e => e.FirstAired).ThenByDescending(e => e.EpisodeName)
                    .GroupBy(e => e.SeriesId.ToString("000000") + e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                    .Select(e => e.First());

            result.Episodes = episodes.ToList();

            return result;
        }
    }
}