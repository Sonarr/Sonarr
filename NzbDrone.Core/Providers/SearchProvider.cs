using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.Providers
{
    public class SearchProvider
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly DownloadProvider _downloadProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly SceneMappingProvider _sceneMappingProvider;
        private readonly UpgradePossibleSpecification _upgradePossibleSpecification;
        private readonly AllowedDownloadSpecification _allowedDownloadSpecification;
        private readonly SearchHistoryProvider _searchHistoryProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public SearchProvider(EpisodeProvider episodeProvider, DownloadProvider downloadProvider, SeriesProvider seriesProvider,
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

        public SearchProvider()
        {
        }

        public virtual List<int> SeasonSearch(ProgressNotification notification, int seriesId, int seasonNumber)
        {
            var searchResult = new SearchHistory
            {
                SearchTime = DateTime.Now,
                SeriesId = seriesId,
                SeasonNumber = seasonNumber
            };

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

            notification.CurrentMessage = String.Format("Searching for {0} Season {1}", series.Title, seasonNumber);

            List<EpisodeParseResult> reports;

            if (series.UseSceneNumbering)
            {
                var sceneSeasonNumbers = episodes.Select(e => e.SceneSeasonNumber).ToList();
                var sceneEpisodeNumbers = episodes.Select(e => e.SceneEpisodeNumber).ToList();

                if (sceneSeasonNumbers.Distinct().Count() > 1)
                {
                    logger.Trace("Uses scene numbering, but multiple seasons found, skipping.");
                    return new List<int>();
                }

                reports = PerformSeasonSearch(series, sceneSeasonNumbers.First());

                reports.Where(p => p.FullSeason && p.SeasonNumber == sceneSeasonNumbers.First()).ToList().ForEach(
                    e => e.EpisodeNumbers = sceneEpisodeNumbers.ToList()
                );
            }

            else
            {
                reports = PerformSeasonSearch(series, seasonNumber);

                reports.Where(p => p.FullSeason && p.SeasonNumber == seasonNumber).ToList().ForEach(
                    e => e.EpisodeNumbers = episodes.Select(ep => ep.EpisodeNumber).ToList()
                );
            }
            
            logger.Debug("Finished searching all indexers. Total {0}", reports.Count);

            if (reports.Count == 0)
                return new List<int>();

            notification.CurrentMessage = "Processing search results";

            searchResult.SearchHistoryItems = ProcessSearchResults(notification, reports, searchResult, series, seasonNumber);
            _searchHistoryProvider.Add(searchResult);

            return searchResult.Successes;
        }

        public virtual List<int> PartialSeasonSearch(ProgressNotification notification, int seriesId, int seasonNumber)
        {
            var searchResult = new SearchHistory
            {
                SearchTime = DateTime.Now,
                SeriesId = seriesId,
                SeasonNumber = seasonNumber
            };

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

            notification.CurrentMessage = String.Format("Searching for {0} Season {1}", series.Title, seasonNumber);
            var episodes = _episodeProvider.GetEpisodesBySeason(seriesId, seasonNumber);

            List<EpisodeParseResult> reports;

            if (series.UseSceneNumbering)
            {
                var sceneSeasonNumbers = episodes.Select(e => e.SceneSeasonNumber).ToList();
                var sceneEpisodeNumbers = episodes.Select(e => e.SceneEpisodeNumber).ToList();

                if (sceneSeasonNumbers.Distinct().Count() > 1)
                {
                    logger.Trace("Uses scene numbering, but multiple seasons found, skipping.");
                    return new List<int>();
                }

                reports = PerformPartialSeasonSearch(series, sceneSeasonNumbers.First(), GetEpisodeNumberPrefixes(sceneEpisodeNumbers));
            }

            else
            {
                reports = PerformPartialSeasonSearch(series, seasonNumber, GetEpisodeNumberPrefixes(episodes.Select(e => e.EpisodeNumber)));
            }
            
            logger.Debug("Finished searching all indexers. Total {0}", reports.Count);

            if (reports.Count == 0)
                return new List<int>();

            notification.CurrentMessage = "Processing search results";
            searchResult.SearchHistoryItems = ProcessSearchResults(notification, reports, searchResult, series, seasonNumber);

            _searchHistoryProvider.Add(searchResult);
            return searchResult.Successes;
        }

        public virtual bool EpisodeSearch(ProgressNotification notification, int episodeId)
        {
            var episode = _episodeProvider.GetEpisode(episodeId);

            if (episode == null)
            {
                logger.Error("Unable to find an episode {0} in database", episodeId);
                return false;
            }

            if (!_upgradePossibleSpecification.IsSatisfiedBy(episode))
            {
                logger.Info("Search for {0} was aborted, file in disk meets or exceeds Profile's Cutoff", episode);
                notification.CurrentMessage = String.Format("Skipping search for {0}, the file you have is already at cutoff", episode);
                return false;
            }

            notification.CurrentMessage = "Looking for " + episode;
            List<EpisodeParseResult> reports;

            var searchResult = new SearchHistory
                                   {
                                        SearchTime = DateTime.Now,
                                        SeriesId = episode.Series.SeriesId,
                                        EpisodeId = episodeId
                                   };

            if (episode.Series.IsDaily)
            {
                if (!episode.AirDate.HasValue)
                {
                    logger.Warn("AirDate is not Valid for: {0}", episode);
                    notification.CurrentMessage = String.Format("Search for {0} Failed, AirDate is invalid", episode);
                    return false;
                }

                reports = PerformDailyEpisodeSearch(episode.Series, episode);

                logger.Debug("Finished searching all indexers. Total {0}", reports.Count);
                notification.CurrentMessage = "Processing search results";

                searchResult.SearchHistoryItems = ProcessSearchResults(notification, reports, episode.Series, episode.AirDate.Value);
                _searchHistoryProvider.Add(searchResult);

                if (searchResult.SearchHistoryItems.Any(r => r.Success))
                    return true;
            }

            else if (episode.Series.UseSceneNumbering)
            {
                var seasonNumber = episode.SceneSeasonNumber;
                var episodeNumber = episode.SceneEpisodeNumber;

                if (seasonNumber == 0 && episodeNumber == 0)
                {
                    seasonNumber = episode.SeasonNumber;
                    episodeNumber = episode.EpisodeNumber;
                }

                reports = PerformEpisodeSearch(episode.Series, seasonNumber, episodeNumber);

                searchResult.SearchHistoryItems = ProcessSearchResults(
                                                                        notification,
                                                                        reports,
                                                                        searchResult,
                                                                        episode.Series,
                                                                        seasonNumber,
                                                                        episodeNumber
                                                                        );

                _searchHistoryProvider.Add(searchResult);

                if (searchResult.SearchHistoryItems.Any(r => r.Success))
                    return true;
            }

            else
            {
                reports = PerformEpisodeSearch(episode.Series, episode.SeasonNumber, episode.EpisodeNumber);

                searchResult.SearchHistoryItems = ProcessSearchResults(notification, reports, searchResult, episode.Series, episode.SeasonNumber, episode.EpisodeNumber);
                _searchHistoryProvider.Add(searchResult);

                if (searchResult.SearchHistoryItems.Any(r => r.Success))
                    return true;
            }

            logger.Warn("Unable to find {0} in any of indexers.", episode);

            notification.CurrentMessage = reports.Any() ? String.Format("Sorry, couldn't find {0}, that matches your preferences.", episode)
                                                        : String.Format("Sorry, couldn't find {0} in any of indexers.", episode);

            return false;
        }

        public List<SearchHistoryItem> ProcessSearchResults(ProgressNotification notification, IEnumerable<EpisodeParseResult> reports, SearchHistory searchResult, Series series, int seasonNumber, int? episodeNumber = null)
        {
            var items = new List<SearchHistoryItem>();
            searchResult.Successes = new List<int>();

            foreach (var episodeParseResult in reports.OrderByDescending(c => c.Quality)
                                                        .ThenBy(c => c.EpisodeNumbers.MinOrDefault())
                                                        .ThenBy(c => c.Age))
            {
                try
                {
                    logger.Trace("Analysing report " + episodeParseResult);

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

                    //Get the matching series
                    episodeParseResult.Series = _seriesProvider.FindSeries(episodeParseResult.CleanTitle);

                    //If series is null or doesn't match the series we're looking for return
                    if (episodeParseResult.Series == null || episodeParseResult.Series.SeriesId != series.SeriesId)
                    {
                        logger.Trace("Unexpected series for search: {0}. Skipping.", episodeParseResult.CleanTitle);
                        item.SearchError = ReportRejectionType.WrongSeries;
                        continue;
                    }

                    //If SeasonNumber doesn't match or episode is not in the in the list in the parse result, skip the report.
                    if (episodeParseResult.SeasonNumber != seasonNumber)
                    {
                        logger.Trace("Season number does not match searched season number, skipping.");
                        item.SearchError = ReportRejectionType.WrongSeason;
                        continue;
                    }

                    //If the EpisodeNumber was passed in and it is not contained in the parseResult, skip the report.
                    if (episodeNumber.HasValue && !episodeParseResult.EpisodeNumbers.Contains(episodeNumber.Value))
                    {
                        logger.Trace("Searched episode number is not contained in post, skipping.");
                        item.SearchError = ReportRejectionType.WrongEpisode;
                        continue;
                    }

                    //Make sure we haven't already downloaded a report with this episodenumber, if we have, skip the report.
                    if (searchResult.Successes.Intersect(episodeParseResult.EpisodeNumbers).Any())
                    {
                        logger.Trace("Episode has already been downloaded in this search, skipping.");
                        item.SearchError = ReportRejectionType.Skipped;
                        continue;
                    }

                    episodeParseResult.Episodes = _episodeProvider.GetEpisodesByParseResult(episodeParseResult);

                    item.SearchError = _allowedDownloadSpecification.IsSatisfiedBy(episodeParseResult);
                    if (item.SearchError == ReportRejectionType.None)
                    {
                        logger.Debug("Found '{0}'. Adding to download queue.", episodeParseResult);
                        try
                        {
                            if (_downloadProvider.DownloadReport(episodeParseResult))
                            {
                                notification.CurrentMessage = String.Format("{0} Added to download queue", episodeParseResult);

                                //Add the list of episode numbers from this release
                                searchResult.Successes.AddRange(episodeParseResult.EpisodeNumbers);
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
                    }
                }
                catch (Exception e)
                {
                    logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }

            return items;
        }

        public List<SearchHistoryItem> ProcessSearchResults(ProgressNotification notification, IEnumerable<EpisodeParseResult> reports, Series series, DateTime airDate)
        {
            var items = new List<SearchHistoryItem>();
            var skip = false;

            foreach (var episodeParseResult in reports.OrderByDescending(c => c.Quality))
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

                    if (skip)
                    {
                        item.SearchError = ReportRejectionType.Skipped;
                        continue;
                    }

                    logger.Trace("Analysing report " + episodeParseResult);

                    //Get the matching series
                    episodeParseResult.Series = _seriesProvider.FindSeries(episodeParseResult.CleanTitle);

                    //If series is null or doesn't match the series we're looking for return
                    if (episodeParseResult.Series == null || episodeParseResult.Series.SeriesId != series.SeriesId)
                    {
                        item.SearchError = ReportRejectionType.WrongSeries;
                        continue;
                    }

                    //If parse result doesn't have an air date or it doesn't match passed in airdate, skip the report.
                    if (!episodeParseResult.AirDate.HasValue || episodeParseResult.AirDate.Value.Date != airDate.Date)
                    {
                        item.SearchError = ReportRejectionType.WrongEpisode;
                        continue;
                    }

                    episodeParseResult.Episodes = _episodeProvider.GetEpisodesByParseResult(episodeParseResult);

                    item.SearchError = _allowedDownloadSpecification.IsSatisfiedBy(episodeParseResult);
                    if (item.SearchError == ReportRejectionType.None)
                    {
                        logger.Debug("Found '{0}'. Adding to download queue.", episodeParseResult);
                        try
                        {
                            if (_downloadProvider.DownloadReport(episodeParseResult))
                            {
                                notification.CurrentMessage =
                                        String.Format("{0} - {1} {2} Added to download queue",
                                                      episodeParseResult.Series.Title, episodeParseResult.AirDate.Value.ToShortDateString(), episodeParseResult.Quality);

                                item.Success = true;
                                skip = true;
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
                    }
                }
                catch (Exception e)
                {
                    logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }

            return items;
        }

        private List<int> GetEpisodeNumberPrefixes(IEnumerable<int> episodeNumbers)
        {
            var results = new List<int>();

            foreach (var i in episodeNumbers)
            {
                results.Add(i / 10);
            }

            return results.Distinct().ToList();
        }

        public List<EpisodeParseResult> PerformEpisodeSearch(Series series, int seasonNumber, int episodeNumber)
        {
            var reports = new List<EpisodeParseResult>();
            var title = GetSeriesTitle(series);

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchEpisode(title, seasonNumber, episodeNumber));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0}-S{1:00}E{2:00} from: {3}",
                                                         series.Title, seasonNumber, episodeNumber, indexer.Name), e);
                }
            });

            return reports;
        }

        public List<EpisodeParseResult> PerformDailyEpisodeSearch(Series series, Episode episode)
        {
            var reports = new List<EpisodeParseResult>();
            var title = GetSeriesTitle(series);

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    logger.Trace("Episode {0} is a daily episode, searching as daily", episode);
                    reports.AddRange(indexer.FetchDailyEpisode(title, episode.AirDate.Value));
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0}-{1} from: {2}",
                                                         series.Title, episode.AirDate, indexer.Name), e);
                }
            });

            return reports;
        }

        public List<EpisodeParseResult> PerformPartialSeasonSearch(Series series, int seasonNumber, List<int> prefixes)
        {
            var reports = new List<EpisodeParseResult>();
            var title = GetSeriesTitle(series);

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    foreach (var episodePrefix in prefixes)
                    {
                        reports.AddRange(indexer.FetchPartialSeason(title, seasonNumber, episodePrefix));
                    }
                }

                catch (Exception e)
                {
                    logger.ErrorException(String.Format("An error has occurred while searching for {0}-S{1:00} from: {2}",
                                                         series.Title, seasonNumber, indexer.Name), e);
                }
            });

            return reports;
        }

        public List<EpisodeParseResult> PerformSeasonSearch(Series series, int seasonNumber)
        {
            var reports = new List<EpisodeParseResult>();
            var title = GetSeriesTitle(series);

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchSeason(title, seasonNumber));
                }

                catch (Exception e)
                {
                    logger.ErrorException("An error has occurred while searching for items from: " + indexer.Name, e);
                }
            });

            return reports;
        }

        public string GetSeriesTitle(Series series)
        {
            var title = _sceneMappingProvider.GetSceneName(series.SeriesId);

            if(String.IsNullOrWhiteSpace(title))
            {
                title = series.Title;
                title = title.Replace("&", "and");
            }

            return title;
        }
    }
}
