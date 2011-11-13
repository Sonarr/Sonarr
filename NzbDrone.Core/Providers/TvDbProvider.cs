using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Ninject;
using NzbDrone.Common;
using TvdbLib;
using TvdbLib.Cache;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class TvDbProvider
    {
        private readonly EnviromentProvider _enviromentProvider;
        private const string TVDB_APIKEY = "5D2D188E86E07F4F";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly TvdbHandler _handler;

        [Inject]
        public TvDbProvider(EnviromentProvider enviromentProvider)
        {
            _enviromentProvider = enviromentProvider;
            _handler = new TvdbHandler(new XmlCacheProvider(_enviromentProvider.GetCacheFolder()), TVDB_APIKEY);
        }

        public TvDbProvider()
        {

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


        public virtual TvdbSeries GetSeries(int id, bool loadEpisodes)
        {
            lock (_handler)
            {
                Logger.Debug("Fetching SeriesId'{0}' from tvdb", id);
                var result = _handler.GetSeries(id, TvdbLanguage.DefaultLanguage, loadEpisodes, false, true, true);

                //Fix American Dad's scene gongshow 
                if (result != null && result.Id == 73141)
                {
                    result.Episodes = result.Episodes.Where(e => e.SeasonNumber == 0 || e.EpisodeNumber > 0).ToList();

                    var seasonOneEpisodeCount = result.Episodes.Where(e => e.SeasonNumber == 1).Count();
                    var seasonOneId = result.Episodes.Where(e => e.SeasonNumber == 1).First().SeasonId;

                    foreach (var episode in result.Episodes)
                    {
                        if (episode.SeasonNumber > 1)
                        {
                            if (episode.SeasonNumber == 2)
                            {
                                episode.EpisodeNumber = episode.EpisodeNumber + seasonOneEpisodeCount;
                                episode.SeasonId = seasonOneId;
                            }

                            episode.SeasonNumber = episode.SeasonNumber - 1;
                        }

                    }
                }

                //Remove duplicated episodes
                var episodes = result.Episodes.OrderByDescending(e => e.FirstAired).ThenByDescending(e => e.EpisodeName)
                     .GroupBy(e => e.SeriesId.ToString("000000") + e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                     .Select(e => e.First());

                result.Episodes = episodes.ToList();

                return result;
            }
        }

    }
}