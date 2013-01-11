using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers.Search
{
    public abstract class SearchBase
    {
        protected readonly EpisodeProvider _episodeProvider;
        protected readonly DownloadProvider _downloadProvider;
        protected readonly SeriesProvider _seriesProvider;
        protected readonly IndexerProvider _indexerProvider;
        protected readonly SceneMappingProvider _sceneMappingProvider;
        protected readonly UpgradePossibleSpecification _upgradePossibleSpecification;
        protected readonly AllowedDownloadSpecification _allowedDownloadSpecification;
        protected readonly SearchHistoryProvider _searchHistoryProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected SearchBase(EpisodeProvider episodeProvider, DownloadProvider downloadProvider,SeriesProvider seriesProvider,
                             IndexerProvider indexerProvider, SceneMappingProvider sceneMappingProvider,
                             UpgradePossibleSpecification upgradePossibleSpecification, AllowedDownloadSpecification allowedDownloadSpecification,
                             SearchHistoryProvider searchHistoryProvider)
        {
            _episodeProvider = episodeProvider;
            _downloadProvider = downloadProvider;
            _seriesProvider = seriesProvider;
            _indexerProvider = indexerProvider;
            _sceneMappingProvider = sceneMappingProvider;
            _upgradePossibleSpecification = upgradePossibleSpecification;
            _allowedDownloadSpecification = allowedDownloadSpecification;
            _searchHistoryProvider = searchHistoryProvider;
        }

        protected SearchBase()
        {
        }

        protected abstract List<EpisodeParseResult> Search(Series series, dynamic options);
        protected abstract SearchHistoryItem CheckEpisode(Series series, List<Episode> episodes, EpisodeParseResult episodeParseResult,
                                                          SearchHistoryItem item);

        protected virtual SearchHistoryItem ProcessReport(EpisodeParseResult episodeParseResult, Series series, List<Episode> episodes)
        {
            try
            {
                var item = new SearchHistoryItem
                {
                    ReportTitle = episodeParseResult.OriginalString,
                    NzbUrl = episodeParseResult.NzbUrl,
                    Indexer = episodeParseResult.Indexer,
                    Quality = episodeParseResult.Quality.Quality,
                    Proper = episodeParseResult.Quality.Proper,
                    Size = episodeParseResult.Size,
                    Age = episodeParseResult.Age,
                    Language = episodeParseResult.Language
                };

                logger.Trace("Analysing report " + episodeParseResult);

                //Get the matching series
                episodeParseResult.Series = _seriesProvider.FindSeries(episodeParseResult.CleanTitle);

                //If series is null or doesn't match the series we're looking for return
                if (episodeParseResult.Series == null || episodeParseResult.Series.SeriesId != series.SeriesId)
                {
                    item.SearchError = ReportRejectionType.WrongSeries;
                    return item;
                }

                //If parse result doesn't have an air date or it doesn't match passed in airdate, skip the report.
                if (CheckEpisode(series, episodes, item).SearchError != ReportRejectionType.None)
                {
                    return item;
                }

                episodeParseResult.Episodes = _episodeProvider.GetEpisodesByParseResult(episodeParseResult);

                item.SearchError = _allowedDownloadSpecification.IsSatisfiedBy(episodeParseResult);
                return item;
            }
            catch (Exception e)
            {
                logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
            }

            return null;
        }

        protected virtual SearchHistoryItem DownloadReport(ProgressNotification notification, EpisodeParseResult episodeParseResult, SearchHistoryItem item)
        {
            //Todo: Customize download message per search type? (override)

            logger.Debug("Found '{0}'. Adding to download queue.", episodeParseResult);
            try
            {
                if (_downloadProvider.DownloadReport(episodeParseResult))
                {
                    notification.CurrentMessage =
                            String.Format("{0} - {1} {2} Added to download queue",
                                            episodeParseResult.Series.Title, episodeParseResult.AirDate.Value.ToShortDateString(), episodeParseResult.Quality);

                    item.Success = true;
                }
                else
                {
                    item.SearchError = ReportRejectionType.DownloadClientFailure;
                }
            }
            catch (Exception e)
            {
                logger.ErrorException("Unable to add report to download queue." + episodeParseResult, e);
                notification.CurrentMessage = String.Format("Unable to add report to download queue. {0}", episodeParseResult);
                item.SearchError = ReportRejectionType.DownloadClientFailure;
            }

            return item;
        }

        protected virtual string GetSeriesTitle(Series series, int seasonNumber = -1)
        {
            //Todo: Add support for per season lookup (used for anime)
            var title = _sceneMappingProvider.GetSceneName(series.SeriesId);

            if (String.IsNullOrWhiteSpace(title))
            {
                title = series.Title;
                title = title.Replace("&", "and");
            }

            return title;
        }
    }
}
