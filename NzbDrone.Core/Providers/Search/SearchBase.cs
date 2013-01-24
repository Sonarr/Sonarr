using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        protected readonly SeriesProvider _seriesProvider;
        protected readonly EpisodeProvider _episodeProvider;
        protected readonly DownloadProvider _downloadProvider;
        protected readonly IndexerProvider _indexerProvider;
        protected readonly SceneMappingProvider _sceneMappingProvider;
        protected readonly AllowedDownloadSpecification _allowedDownloadSpecification;
        protected readonly SearchHistoryProvider _searchHistoryProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected SearchBase(SeriesProvider seriesProvider, EpisodeProvider episodeProvider, DownloadProvider downloadProvider,
                             IndexerProvider indexerProvider, SceneMappingProvider sceneMappingProvider,
                             AllowedDownloadSpecification allowedDownloadSpecification,
                             SearchHistoryProvider searchHistoryProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _downloadProvider = downloadProvider;
            _indexerProvider = indexerProvider;
            _sceneMappingProvider = sceneMappingProvider;
            _allowedDownloadSpecification = allowedDownloadSpecification;
            _searchHistoryProvider = searchHistoryProvider;
        }

        protected SearchBase()
        {
        }

        public abstract List<EpisodeParseResult> PerformSearch(Series series, dynamic options, ProgressNotification notification);
        public abstract SearchHistoryItem CheckReport(Series series, dynamic options, EpisodeParseResult episodeParseResult,
                                                                SearchHistoryItem item);

        protected abstract void FinalizeSearch(Series series, dynamic options, Boolean reportsFound, ProgressNotification notification);

        public virtual List<Int32> Search(Series series, dynamic options, ProgressNotification notification)
        {
            if (options == null)
                throw new ArgumentNullException(options);

            var searchResult = new SearchHistory
            {
                SearchTime = DateTime.Now,
                SeriesId = series.SeriesId,
                EpisodeId = options.GetType().GetProperty("Episode") != null ? options.Episode.EpisodeId : null,
                SeasonNumber = options.GetType().GetProperty("SeasonNumber") != null ? options.SeasonNumber : null
            };

            List<EpisodeParseResult> reports = PerformSearch(series, options, notification);
            
            logger.Debug("Finished searching all indexers. Total {0}", reports.Count);
            notification.CurrentMessage = "Processing search results";
            
            ProcessReports(series, options, reports, searchResult, notification);
            _searchHistoryProvider.Add(searchResult);

            if(searchResult.Successes.Any())
                return searchResult.Successes;

            FinalizeSearch(series, options, reports.Any(), notification);
            return new List<Int32>();
        }

        public virtual SearchHistory ProcessReports(Series series, dynamic options, List<EpisodeParseResult> episodeParseResults,
                                                              SearchHistory searchResult, ProgressNotification notification)
        {
            var items = new List<SearchHistoryItem>();
            searchResult.Successes = new List<Int32>();

            foreach(var episodeParseResult in episodeParseResults
                                                        .OrderByDescending(c => c.Quality)
                                                        .ThenBy(c => c.EpisodeNumbers.MinOrDefault())
                                                        .ThenBy(c => c.Age))
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

                    items.Add(item);

                    logger.Trace("Analysing report " + episodeParseResult);
                    episodeParseResult.Series = _seriesProvider.FindSeries(episodeParseResult.CleanTitle);

                    if(episodeParseResult.Series == null || episodeParseResult.Series.SeriesId != series.SeriesId)
                    {
                        item.SearchError = ReportRejectionType.WrongSeries;
                        continue;
                    }

                    episodeParseResult.Episodes = _episodeProvider.GetEpisodesByParseResult(episodeParseResult);

                    if (searchResult.Successes.Intersect(episodeParseResult.Episodes.Select(e => e.EpisodeId)).Any())
                    {
                        item.SearchError = ReportRejectionType.Skipped;
                        continue;
                    }

                    CheckReport(series, options, episodeParseResult, item);
                    if (item.SearchError != ReportRejectionType.None)
                        continue;

                    item.SearchError = _allowedDownloadSpecification.IsSatisfiedBy(episodeParseResult);

                    if(item.SearchError == ReportRejectionType.None)
                    {
                        if(DownloadReport(notification, episodeParseResult, item))
                            searchResult.Successes.AddRange(episodeParseResult.Episodes.Select(e => e.EpisodeId));
                    }
                }
                catch(Exception e)
                {
                    logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }

            searchResult.SearchHistoryItems = items;
            return searchResult;
        }

        public virtual Boolean DownloadReport(ProgressNotification notification, EpisodeParseResult episodeParseResult, SearchHistoryItem item)
        {
            logger.Debug("Found '{0}'. Adding to download queue.", episodeParseResult);
            try
            {
                if (_downloadProvider.DownloadReport(episodeParseResult))
                {
                    notification.CurrentMessage = String.Format("{0} Added to download queue", episodeParseResult);
                    item.Success = true;
                    return true;
                }

                item.SearchError = ReportRejectionType.DownloadClientFailure;
            }
            catch (Exception e)
            {
                logger.ErrorException("Unable to add report to download queue." + episodeParseResult, e);
                notification.CurrentMessage = String.Format("Unable to add report to download queue. {0}", episodeParseResult);
                item.SearchError = ReportRejectionType.DownloadClientFailure;
            }

            return false;
        }

        public virtual string GetSearchTitle(Series series, int seasonNumber = -1)
        {
            var seasonTitle = _sceneMappingProvider.GetSceneName(series.SeriesId, seasonNumber);

            if(!String.IsNullOrWhiteSpace(seasonTitle))
                return seasonTitle;

            var title = _sceneMappingProvider.GetSceneName(series.SeriesId);

            if (String.IsNullOrWhiteSpace(title))
            {
                title = series.Title;
                title = title.Replace("&", "and");
                title = Regex.Replace(title, @"[^\w\d\s\-]", "");
            }

            return title;
        }
    }
}
