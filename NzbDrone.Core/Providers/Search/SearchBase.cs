using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers.Search
{
    public abstract class SearchBase
    {
        private readonly ISeriesRepository _seriesRepository;
        protected readonly IEpisodeService _episodeService;
        protected readonly DownloadProvider _downloadProvider;
        protected readonly IIndexerService _indexerService;
        protected readonly SceneMappingService _sceneMappingService;
        protected readonly AllowedDownloadSpecification _allowedDownloadSpecification;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected SearchBase(ISeriesRepository seriesRepository, IEpisodeService episodeService, DownloadProvider downloadProvider,
                             IIndexerService indexerService, SceneMappingService sceneMappingService,
                             AllowedDownloadSpecification allowedDownloadSpecification)
        {
            _seriesRepository = seriesRepository;
            _episodeService = episodeService;
            _downloadProvider = downloadProvider;
            _indexerService = indexerService;
            _sceneMappingService = sceneMappingService;
            _allowedDownloadSpecification = allowedDownloadSpecification;
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
                SeriesId = series.Id,
                EpisodeId = options.GetType().GetProperty("Episode") != null ? options.Episode.EpisodeId : null,
                SeasonNumber = options.GetType().GetProperty("SeasonNumber") != null ? options.SeasonNumber : null
            };

            List<EpisodeParseResult> reports = PerformSearch(series, options, notification);
            
            logger.Debug("Finished searching all indexers. Total {0}", reports.Count);
            notification.CurrentMessage = "Processing search results";
            
            ProcessReports(series, options, reports, searchResult, notification);

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
                    episodeParseResult.Series = _seriesRepository.GetByTitle(episodeParseResult.CleanTitle);

                    if(episodeParseResult.Series == null || ((ModelBase)episodeParseResult.Series).Id != series.Id)
                    {
                        item.SearchError = ReportRejectionReasons.WrongSeries;
                        continue;
                    }

                    episodeParseResult.Episodes = _episodeService.GetEpisodesByParseResult(episodeParseResult);

                    if (searchResult.Successes.Intersect(episodeParseResult.Episodes.Select(e => e.Id)).Any())
                    {
                        item.SearchError = ReportRejectionReasons.Skipped;
                        continue;
                    }

                    CheckReport(series, options, episodeParseResult, item);
                    if (item.SearchError != ReportRejectionReasons.None)
                        continue;

                    item.SearchError = _allowedDownloadSpecification.IsSatisfiedBy(episodeParseResult);

                    if(item.SearchError == ReportRejectionReasons.None)
                    {
                        if(DownloadReport(notification, episodeParseResult, item))
                            searchResult.Successes.AddRange(episodeParseResult.Episodes.Select(e => e.Id));
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

                item.SearchError = ReportRejectionReasons.DownloadClientFailure;
            }
            catch (Exception e)
            {
                logger.ErrorException("Unable to add report to download queue." + episodeParseResult, e);
                notification.CurrentMessage = String.Format("Unable to add report to download queue. {0}", episodeParseResult);
                item.SearchError = ReportRejectionReasons.DownloadClientFailure;
            }

            return false;
        }

        public virtual string GetSearchTitle(Series series, int seasonNumber = -1)
        {
            var seasonTitle = _sceneMappingService.GetSceneName(series.Id, seasonNumber);

            if(!String.IsNullOrWhiteSpace(seasonTitle))
                return seasonTitle;

            var title = _sceneMappingService.GetSceneName(series.Id);

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
