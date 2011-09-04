using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class SearchProvider
    {
        //Season and Episode Searching
        private readonly EpisodeProvider _episodeProvider;
        private readonly InventoryProvider _inventoryProvider;
        private readonly DownloadProvider _downloadProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly SceneMappingProvider _sceneMappingProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public SearchProvider(EpisodeProvider episodeProvider, InventoryProvider inventoryProvider,
                                DownloadProvider downloadProvider, SeriesProvider seriesProvider,
                                IndexerProvider indexerProvider, SceneMappingProvider sceneMappingProvider)
        {
            _episodeProvider = episodeProvider;
            _inventoryProvider = inventoryProvider;
            _downloadProvider = downloadProvider;
            _seriesProvider = seriesProvider;
            _indexerProvider = indexerProvider;
            _sceneMappingProvider = sceneMappingProvider;
        }

        public SearchProvider()
        {
        }

        public virtual bool SeasonSearch(ProgressNotification notification, int seriesId, int seasonNumber)
        {
            var series = _seriesProvider.GetSeries(seriesId);

            if (series == null)
            {
                Logger.Error("Unable to find an series {0} in database", seriesId);
                return false;
            }

            notification.CurrentMessage = String.Format("Searching for {0} Season {1}", series.Title, seasonNumber);

            var indexers = _indexerProvider.GetEnabledIndexers();
            var reports = new List<EpisodeParseResult>();

            var title = _sceneMappingProvider.GetSceneName(series.SeriesId);

            if (string.IsNullOrWhiteSpace(title))
            {
                title = series.Title;
            }

            foreach (var indexer in indexers)
            {
                try
                {
                    var indexerResults = indexer.FetchSeason(title, seasonNumber);

                    reports.AddRange(indexerResults);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while fetching items from " + indexer.Name, e);
                }
            }

            Logger.Debug("Finished searching all indexers. Total {0}", reports.Count);

            if (reports.Count == 0)
                return false;

            Logger.Debug("Getting episodes from database for series: {0} and season: {1}", seriesId, seasonNumber);
            var episodeNumbers = _episodeProvider.GetEpisodeNumbersBySeason(seriesId, seasonNumber);

            if (episodeNumbers == null || episodeNumbers.Count == 0)
            {
                Logger.Warn("No episodes in database found for series: {0} and season: {1}.", seriesId, seasonNumber);
                return false;
            }

            notification.CurrentMessage = "Processing search results";

            var reportsToProcess = reports.Where(p => p.FullSeason && p.SeasonNumber == seasonNumber).ToList();

            reportsToProcess.ForEach(c =>
            {
                c.Series = series;
                c.EpisodeNumbers = episodeNumbers.ToList();
            });

            return ProcessSeasonSearchResults(notification, series, seasonNumber, reportsToProcess);
        }

        public bool ProcessSeasonSearchResults(ProgressNotification notification, Series series, int seasonNumber, IEnumerable<EpisodeParseResult> reports)
        {
            foreach (var episodeParseResult in reports.OrderByDescending(c => c.Quality))
            {
                try
                {
                    Logger.Trace("Analysing report " + episodeParseResult);
                    if (_inventoryProvider.IsQualityNeeded(episodeParseResult))
                    {
                        Logger.Debug("Found '{0}'. Adding to download queue.", episodeParseResult);
                        try
                        {
                            _downloadProvider.DownloadReport(episodeParseResult);
                            notification.CurrentMessage = String.Format("{0} Season {1} {2} Added to download queue", series.Title, seasonNumber, episodeParseResult.Quality);
                        }
                        catch (Exception e)
                        {
                            Logger.ErrorException("Unable to add report to download queue." + episodeParseResult, e);
                            notification.CurrentMessage = String.Format("Unable to add report to download queue. {0}", episodeParseResult);
                        }

                        return true;
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }

            Logger.Warn("Unable to find {0} Season {1} in any of indexers.", series.Title, seasonNumber);
            notification.CurrentMessage = String.Format("Unable to find {0} Season {1} in any of indexers.", series.Title, seasonNumber);

            return false;
        }

        public virtual bool EpisodeSearch(ProgressNotification notification, int episodeId)
        {
            var episode = _episodeProvider.GetEpisode(episodeId);

            if (episode == null)
            {
                Logger.Error("Unable to find an episode {0} in database", episodeId);
                return false;
            }
            notification.CurrentMessage = "Searching for " + episode;


            var series = episode.Series;

            var indexers = _indexerProvider.GetEnabledIndexers();
            var reports = new List<EpisodeParseResult>();

            var title = _sceneMappingProvider.GetSceneName(series.SeriesId);

            if (string.IsNullOrWhiteSpace(title))
            {
                title = series.Title;
            }

            foreach (var indexer in indexers)
            {
                try
                {
                    //notification.CurrentMessage = String.Format("Searching for {0} in {1}", episode, indexer.Name);

                    //TODO:Add support for daily episodes, maybe search using both date and season/episode?
                    var indexerResults = indexer.FetchEpisode(title, episode.SeasonNumber, episode.EpisodeNumber);

                    reports.AddRange(indexerResults);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while fetching items from " + indexer.Name, e);
                }
            }

            Logger.Debug("Finished searching all indexers. Total {0}", reports.Count);
            notification.CurrentMessage = "Processing search results";


            //TODO:fix this so when search returns more than one episode
            //TODO:-its populated with more than the original episode.
            reports.ForEach(c =>
            {
                c.Series = series;
            });

            return ProcessEpisodeSearchResults(notification, episode, reports);
        }

        public bool ProcessEpisodeSearchResults(ProgressNotification notification, Episode episode, IEnumerable<EpisodeParseResult> reports)
        {
            foreach (var episodeParseResult in reports.OrderByDescending(c => c.Quality))
            {
                try
                {
                    Logger.Trace("Analysing report " + episodeParseResult);
                    if (_inventoryProvider.IsQualityNeeded(episodeParseResult))
                    {
                        Logger.Debug("Found '{0}'. Adding to download queue.", episodeParseResult);
                        try
                        {
                            _downloadProvider.DownloadReport(episodeParseResult);
                            notification.CurrentMessage = String.Format("{0} {1} Added to download queue", episode, episodeParseResult.Quality);
                        }
                        catch (Exception e)
                        {
                            Logger.ErrorException("Unable to add report to download queue." + episodeParseResult, e);
                            notification.CurrentMessage = String.Format("Unable to add report to download queue. {0}", episodeParseResult);
                        }

                        return true;
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }

            Logger.Warn("Unable to find {0} in any of indexers.", episode);
            notification.CurrentMessage = String.Format("Unable to find {0} in any of indexers.", episode);

            return false;
        }

        public virtual List<int> PartialSeasonSearch(ProgressNotification notification, int seriesId, int seasonNumber)
        {
            //This method will search for episodes in a season in groups of 10 episodes S01E0, S01E1, S01E2, etc 

            var series = _seriesProvider.GetSeries(seriesId);

            if (series == null)
            {
                Logger.Error("Unable to find an series {0} in database", seriesId);
                return new List<int>();
            }

            notification.CurrentMessage = String.Format("Searching for {0} Season {1}", series.Title, seasonNumber);

            var indexers = _indexerProvider.GetEnabledIndexers();
            var reports = new List<EpisodeParseResult>();

            var title = _sceneMappingProvider.GetSceneName(series.SeriesId);

            if (string.IsNullOrWhiteSpace(title))
            {
                title = series.Title;
            }

            var episodes = _episodeProvider.GetEpisodesBySeason(seriesId, seasonNumber);
            var episodeCount = episodes.Count;
            var episodePrefix = 0;

            while(episodeCount >= 0)
            {
                //Do the actual search for each indexer
                foreach (var indexer in indexers)
                {
                    try
                    {
                        var indexerResults = indexer.FetchPartialSeason(title, seasonNumber, episodePrefix);

                        reports.AddRange(indexerResults);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorException("An error has occurred while fetching items from " + indexer.Name, e);
                    }
                }

                episodePrefix++;
                episodeCount -= 10;
            }

            Logger.Debug("Finished searching all indexers. Total {0}", reports.Count);

            if (reports.Count == 0)
                return new List<int>();

            notification.CurrentMessage = "Processing search results";

            reports.ForEach(c =>
            {
                c.Series = series;
            });

            return  ProcessPartialSeasonSearchResults(notification, reports);
        }

        public List<int> ProcessPartialSeasonSearchResults(ProgressNotification notification, IEnumerable<EpisodeParseResult> reports)
        {
            var successes = new List<int>();

            foreach (var episodeParseResult in reports.OrderByDescending(c => c.Quality))
            {
                try
                {
                    Logger.Trace("Analysing report " + episodeParseResult);
                    if (_inventoryProvider.IsQualityNeeded(episodeParseResult))
                    {
                        Logger.Debug("Found '{0}'. Adding to download queue.", episodeParseResult);
                        try
                        {
                            _downloadProvider.DownloadReport(episodeParseResult);
                            notification.CurrentMessage = String.Format("{0} - S{1:00}E{2:00} {3}Added to download queue",
                                episodeParseResult.Series.Title, episodeParseResult.SeasonNumber, episodeParseResult.EpisodeNumbers[0], episodeParseResult.Quality);

                            //Add the list of episode numbers from this release
                            successes.AddRange(episodeParseResult.EpisodeNumbers);
                        }
                        catch (Exception e)
                        {
                            Logger.ErrorException("Unable to add report to download queue." + episodeParseResult, e);
                            notification.CurrentMessage = String.Format("Unable to add report to download queue. {0}", episodeParseResult);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }

            return successes;
        }
    }
}
