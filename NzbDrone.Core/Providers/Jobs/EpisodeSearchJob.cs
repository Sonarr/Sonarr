using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class EpisodeSearchJob : IJob
    {
        private readonly InventoryProvider _inventoryProvider;
        private readonly DownloadProvider _downloadProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly SceneMappingProvider _sceneNameMappingProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public EpisodeSearchJob(InventoryProvider inventoryProvider, DownloadProvider downloadProvider,
                                    IndexerProvider indexerProvider, EpisodeProvider episodeProvider,
                                    SceneMappingProvider sceneNameMappingProvider)
        {
            _inventoryProvider = inventoryProvider;
            _downloadProvider = downloadProvider;
            _indexerProvider = indexerProvider;
            _episodeProvider = episodeProvider;
            _sceneNameMappingProvider = sceneNameMappingProvider;
        }

        public string Name
        {
            get { return "Episode Search"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            var episode = _episodeProvider.GetEpisode(targetId);

            if (episode == null)
            {
                Logger.Error("Unable to find an episode {0} in database", targetId);
                return;
            }

            var series = episode.Series;

            var indexers = _indexerProvider.GetEnabledIndexers();
            var reports = new List<EpisodeParseResult>();

            var title = _sceneNameMappingProvider.GetSceneName(series.SeriesId);

            if (string.IsNullOrWhiteSpace(title))
            {
                title = series.Title;
            }

            foreach (var indexer in indexers)
            {
                try
                {
                    notification.CurrentMessage = String.Format("Searching for {0} in {1}", episode, indexer.Name);

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

            reports.ForEach(c =>
                                {
                                    c.Series = series;
                                    c.Episodes = new List<Episode> { episode };
                                });

            ProcessResults(notification, episode, reports);
        }

        public void ProcessResults(ProgressNotification notification, Episode episode, IEnumerable<EpisodeParseResult> reports)
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

                        return;
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }

            Logger.Warn("Unable to find {0} in any of indexers.", episode);
            notification.CurrentMessage = String.Format("Unable to find {0} in any of indexers.", episode);
        }
    }
}