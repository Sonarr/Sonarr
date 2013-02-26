using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Tv;
using TvdbLib;
using TvdbLib.Cache;
using TvdbLib.Data;
using TvdbLanguage = TvdbLib.Data.TvdbLanguage;

namespace NzbDrone.Core.Providers
{
    public class TvDbProvider
    {
        private readonly EnvironmentProvider _environmentProvider;
        public const string TVDB_APIKEY = "5D2D188E86E07F4F";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();



        private readonly TvdbHandler _handler;
        private readonly Tvdb.Tvdb _handlerV2;


        public TvDbProvider(EnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
            _handler = new TvdbHandler(new XmlCacheProvider(_environmentProvider.GetCacheFolder()), TVDB_APIKEY);
            _handlerV2 = new Tvdb.Tvdb(TVDB_APIKEY);
        }

        public TvDbProvider()
        {

        }

        public virtual List<Series> SearchSeries(string title)
        {
            logger.Debug("Searching TVDB for '{0}'", title);

            if (title.Contains(" & "))
            {
                title = title.Replace(" & ", " ");
            }

            var searchResult = _handlerV2.SearchSeries(title).Select(tvdbSeriesSearchItem => new Series
                {
                    TvDbId = tvdbSeriesSearchItem.seriesid,
                    Title = tvdbSeriesSearchItem.SeriesName,
                    FirstAired = tvdbSeriesSearchItem.FirstAired,
                    Overview = tvdbSeriesSearchItem.Overview
                }).ToList();

            logger.Debug("Search for '{0}' returned {1} possible results", title, searchResult.Count);
            return searchResult;
        }

        public virtual TvdbSeries GetSeries(int tvDbSeriesId, bool loadEpisodes, bool loadActors = false)
        {
            lock (_handler)
            {
                logger.Debug("Fetching SeriesId'{0}' from tvdb", tvDbSeriesId);
                var result = _handler.GetSeries(tvDbSeriesId, TvdbLanguage.DefaultLanguage, loadEpisodes, loadActors, true, true);

                //Remove duplicated episodes
                var episodes = result.Episodes.OrderByDescending(e => e.FirstAired).ThenByDescending(e => e.EpisodeName)
                     .GroupBy(e => e.SeriesId.ToString("000000") + e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                     .Select(e => e.First());

                result.Episodes = episodes.Where(episode => !string.IsNullOrWhiteSpace(episode.EpisodeName) || (episode.FirstAired < DateTime.Now.AddDays(2) && episode.FirstAired.Year > 1900)).ToList();

                return result;
            }
        }

        public virtual IList<Episode> GetEpisodes(int tvDbSeriesId)
        {
            var series = GetSeries(tvDbSeriesId, true);

            var episodes = new List<Episode>();

            foreach (var tvDbEpisode in series.Episodes)
            {
                var episode = new Episode();

                episode.TvDbEpisodeId = tvDbEpisode.Id;
                episode.EpisodeNumber = tvDbEpisode.EpisodeNumber;
                episode.SeasonNumber = tvDbEpisode.SeasonNumber;
                episode.AbsoluteEpisodeNumber = tvDbEpisode.AbsoluteNumber;
                episode.Title = tvDbEpisode.EpisodeName;
                episode.Overview = tvDbEpisode.Overview;

                if (tvDbEpisode.FirstAired.Year > 1900)
                {
                    episode.AirDate = tvDbEpisode.FirstAired.Date;
                }
                else
                {
                    episode.AirDate = null;
                }
            }

            return episodes;
        }
    }
}