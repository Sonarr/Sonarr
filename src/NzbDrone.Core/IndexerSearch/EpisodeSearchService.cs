using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.IndexerSearch
{
    public interface IEpisodeSearchService
    {
        void MissingEpisodesAiredAfter(DateTime dateTime, IEnumerable<Int32> grabbed);
    }

    public class EpisodeSearchService : IEpisodeSearchService, 
                                        IExecute<EpisodeSearchCommand>, 
                                        IExecute<MissingEpisodeSearchCommand>, 
                                        IHandle<EpisodeInfoRefreshedEvent>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IEpisodeService _episodeService;
        private readonly IQueueService _queueService;
        private readonly Logger _logger;

        public EpisodeSearchService(ISearchForNzb nzbSearchService,
                                    IProcessDownloadDecisions processDownloadDecisions,
                                    IEpisodeService episodeService,
                                    IQueueService queueService,
                                    Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _episodeService = episodeService;
            _queueService = queueService;
            _logger = logger;
        }

        public void MissingEpisodesAiredAfter(DateTime dateTime, IEnumerable<Int32> grabbed)
        {
            var missing = _episodeService.EpisodesBetweenDates(dateTime, DateTime.UtcNow)
                                         .Where(e => !e.HasFile &&
                                                !_queueService.GetQueue().Select(q => q.Episode.Id).Contains(e.Id) &&
                                                !grabbed.Contains(e.Id))
                                         .ToList();

            var downloadedCount = 0;
            _logger.Info("Searching for {0} missing episodes since last RSS Sync", missing.Count);

            foreach (var episode in missing)
            {
                //TODO: Add a flag to the search to state it is a "scheduled" search
                var decisions = _nzbSearchService.EpisodeSearch(episode);
                var processed = _processDownloadDecisions.ProcessDecisions(decisions);
                downloadedCount += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Completed search for {0} episodes. {1} reports downloaded.", missing.Count, downloadedCount);
        }

        private void SearchForMissingEpisodes(List<Episode> episodes)
        {
            _logger.ProgressInfo("Performing missing search for {0} episodes", episodes.Count);
            var downloadedCount = 0;

            foreach (var series in episodes.GroupBy(e => e.SeriesId))
            {
                foreach (var season in series.Select(e => e).GroupBy(e => e.SeasonNumber))
                {
                    List<DownloadDecision> decisions;

                    if (season.Count() > 1)
                    {
                        decisions = _nzbSearchService.SeasonSearch(series.Key, season.Key);
                    }

                    else
                    {
                        decisions = _nzbSearchService.EpisodeSearch(season.First());
                    }

                    var processed = _processDownloadDecisions.ProcessDecisions(decisions);

                    downloadedCount += processed.Grabbed.Count;
                }
            }

            _logger.ProgressInfo("Completed missing search for {0} episodes. {1} reports downloaded.", episodes.Count, downloadedCount);
        }
        
        public void Execute(EpisodeSearchCommand message)
        {
            foreach (var episodeId in message.EpisodeIds)
            {
                var decisions = _nzbSearchService.EpisodeSearch(episodeId);
                var processed = _processDownloadDecisions.ProcessDecisions(decisions);

                _logger.ProgressInfo("Episode search completed. {0} reports downloaded.", processed.Grabbed.Count);
            }
        }

        public void Execute(MissingEpisodeSearchCommand message)
        {
            List<Episode> episodes;

            if (message.SeriesId.HasValue)
            {
                episodes = _episodeService.GetEpisodeBySeries(message.SeriesId.Value)
                                          .Where(e => e.Monitored &&
                                                 !e.HasFile &&
                                                 e.AirDateUtc.HasValue &&
                                                 e.AirDateUtc.Value.Before(DateTime.UtcNow))
                                          .ToList();
            }

            else
            {
                episodes = _episodeService.EpisodesWithoutFiles(new PagingSpec<Episode>
                                                                    {
                                                                        Page = 1,
                                                                        PageSize = 100000,
                                                                        SortDirection = SortDirection.Ascending,
                                                                        SortKey = "Id",
                                                                        FilterExpression =
                                                                            v =>
                                                                            v.Monitored == true &&
                                                                            v.Series.Monitored == true
                                                                    }).Records.ToList();
            }

            var queue = _queueService.GetQueue().Select(q => q.Episode.Id);
            var missing = episodes.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForMissingEpisodes(missing);
        }

        public void Handle(EpisodeInfoRefreshedEvent message)
        {
            //TODO: This should be triggered off of a disk scan, that follows after the refresh so existing files on disk are counted
            return;

            if (!message.Series.Monitored)
            {
                _logger.Debug("Series is not monitored");
                return;
            }

            if (message.Updated.Empty() || message.Series.Added.InLastDays(1))
            {
                _logger.Debug("Appears to be a new series, skipping search.");
                return;
            }

            if (message.Added.Empty())
            {
                _logger.Debug("No new episodes, skipping search");
                return;
            }

            if (message.Added.None(a => a.AirDateUtc.HasValue))
            {
                _logger.Debug("No new episodes have an air date");
                return;
            }

            var previouslyAired = message.Added.Where(a => a.AirDateUtc.HasValue && a.AirDateUtc.Value.Before(DateTime.UtcNow.AddDays(1))).ToList();

            if (previouslyAired.Empty())
            {
                _logger.Debug("Newly added episodes all air in the future");
                return;
            }

            foreach (var episode in previouslyAired)
            {
                if (!episode.Monitored)
                {
                    _logger.Debug("Episode is not monitored");
                    continue;
                }

                var decisions = _nzbSearchService.EpisodeSearch(episode);
                var processed = _processDownloadDecisions.ProcessDecisions(decisions);

                _logger.ProgressInfo("Episode search completed. {0} reports downloaded.", processed.Grabbed.Count);
            }
        }
    }
}
