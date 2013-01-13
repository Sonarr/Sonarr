using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers
{
    public class SearchProvider
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly PartialSeasonSearch _partialSeasonSearch;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SearchProvider(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
                              PartialSeasonSearch partialSeasonSearch)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _partialSeasonSearch = partialSeasonSearch;
        }

        public SearchProvider()
        {
        }

        public virtual List<int> SeasonSearch(ProgressNotification notification, int seriesId, int seasonNumber)
        {
            var series = _seriesProvider.GetSeries(seriesId);

            if (series == null)
            {
                logger.Error("Unable to find an series {0} in database", seriesId);
                return new List<int>();
            }

            if (series.IsDaily)
            {
                logger.Trace("Daily series detected, skipping season search: {0}", series.Title);
                return new List<int>();
            }

            logger.Debug("Getting episodes from database for series: {0} and season: {1}", seriesId, seasonNumber);
            var episodes = _episodeProvider.GetEpisodesBySeason(seriesId, seasonNumber);

            if (episodes == null || episodes.Count == 0)
            {
                logger.Warn("No episodes in database found for series: {0} and season: {1}.", seriesId, seasonNumber);
                return new List<int>();
            }

            //Todo: Support full season searching
            return new List<int>();
        }

        public virtual List<int> PartialSeasonSearch(ProgressNotification notification, int seriesId, int seasonNumber)
        {
            var series = _seriesProvider.GetSeries(seriesId);

            if (series == null)
            {
                logger.Error("Unable to find an series {0} in database", seriesId);
                return new List<int>();
            }

            if (series.IsDaily)
            {
                logger.Trace("Daily series detected, skipping season search: {0}", series.Title);
                return new List<int>();
            }

            var episodes = _episodeProvider.GetEpisodesBySeason(seriesId, seasonNumber);

            if (episodes == null || episodes.Count == 0)
            {
                logger.Warn("No episodes in database found for series: {0} Season: {1}.", seriesId, seasonNumber);
                return new List<int>();
            }

            return _partialSeasonSearch.Search(series, new {SeasonNumber = seasonNumber, Episodes = episodes}, notification);
        }
    }
}
