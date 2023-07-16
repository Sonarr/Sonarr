using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public class EpisodeSearchService : IExecute<EpisodeSearchCommand>,
                                        IExecute<MissingEpisodeSearchCommand>,
                                        IExecute<CutoffUnmetEpisodeSearchCommand>
    {
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IEpisodeService _episodeService;
        private readonly IEpisodeCutoffService _episodeCutoffService;
        private readonly IQueueService _queueService;
        private readonly Logger _logger;

        public EpisodeSearchService(ISearchForReleases releaseSearchService,
                                    IProcessDownloadDecisions processDownloadDecisions,
                                    IEpisodeService episodeService,
                                    IEpisodeCutoffService episodeCutoffService,
                                    IQueueService queueService,
                                    Logger logger)
        {
            _releaseSearchService = releaseSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _episodeService = episodeService;
            _episodeCutoffService = episodeCutoffService;
            _queueService = queueService;
            _logger = logger;
        }

        private async Task SearchForBulkEpisodes(List<Episode> episodes, bool monitoredOnly, bool userInvokedSearch)
        {
            _logger.ProgressInfo("Performing search for {0} episodes", episodes.Count);
            var downloadedCount = 0;
            var groups = new List<EpisodeSearchGroup>();

            foreach (var series in episodes.GroupBy(e => e.SeriesId))
            {
                foreach (var season in series.Select(e => e).GroupBy(e => e.SeasonNumber))
                {
                    groups.Add(new EpisodeSearchGroup
                    {
                        SeriesId = series.Key,
                        SeasonNumber = season.Key,
                        Episodes = season.ToList()
                    });
                }
            }

            foreach (var group in groups.OrderBy(g => g.Episodes.Min(e => e.LastSearchTime ?? DateTime.MinValue)))
            {
                List<DownloadDecision> decisions;

                var seriesId = group.SeriesId;
                var seasonNumber = group.SeasonNumber;
                var groupEpisodes = group.Episodes;

                if (groupEpisodes.Count > 1)
                {
                    try
                    {
                        decisions = await _releaseSearchService.SeasonSearch(seriesId, seasonNumber, groupEpisodes, monitoredOnly, userInvokedSearch, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Unable to search for episodes in season {0} of [{1}]", seasonNumber, seriesId);
                        continue;
                    }
                }
                else
                {
                    var episode = groupEpisodes.First();

                    try
                    {
                        decisions = await _releaseSearchService.EpisodeSearch(episode, userInvokedSearch, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Unable to search for episode: [{0}]", episode);
                        continue;
                    }
                }

                var processed = await _processDownloadDecisions.ProcessDecisions(decisions);

                downloadedCount += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Completed search for {0} episodes. {1} reports downloaded.", episodes.Count, downloadedCount);
        }

        private bool IsMonitored(bool episodeMonitored, bool seriesMonitored)
        {
            return episodeMonitored && seriesMonitored;
        }

        public void Execute(EpisodeSearchCommand message)
        {
            foreach (var episodeId in message.EpisodeIds)
            {
                var decisions = _releaseSearchService.EpisodeSearch(episodeId, message.Trigger == CommandTrigger.Manual, false).GetAwaiter().GetResult();
                var processed = _processDownloadDecisions.ProcessDecisions(decisions).GetAwaiter().GetResult();

                _logger.ProgressInfo("Episode search completed. {0} reports downloaded.", processed.Grabbed.Count);
            }
        }

        public void Execute(MissingEpisodeSearchCommand message)
        {
            var monitored = message.Monitored;
            List<Episode> episodes;

            if (message.SeriesId.HasValue)
            {
                episodes = _episodeService.GetEpisodeBySeries(message.SeriesId.Value)
                                          .Where(e => e.Monitored == monitored &&
                                                 !e.HasFile &&
                                                 e.AirDateUtc.HasValue &&
                                                 e.AirDateUtc.Value.Before(DateTime.UtcNow))
                                          .ToList();
            }
            else
            {
                var pagingSpec = new PagingSpec<Episode>
                                 {
                                     Page = 1,
                                     PageSize = 1000000,
                                     SortDirection = SortDirection.Ascending,
                                     SortKey = "Id"
                                 };

                if (monitored)
                {
                    pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Series.Monitored == true);
                }
                else
                {
                    pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Series.Monitored == false);
                }

                episodes = _episodeService.EpisodesWithoutFiles(pagingSpec).Records.ToList();
            }

            var queue = _queueService.GetQueue().Where(q => q.Episode != null).Select(q => q.Episode.Id);
            var missing = episodes.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForBulkEpisodes(missing, monitored, message.Trigger == CommandTrigger.Manual).GetAwaiter().GetResult();
        }

        public void Execute(CutoffUnmetEpisodeSearchCommand message)
        {
            var monitored = message.Monitored;

            var pagingSpec = new PagingSpec<Episode>
                             {
                                 Page = 1,
                                 PageSize = 100000,
                                 SortDirection = SortDirection.Ascending,
                                 SortKey = "Id"
                             };

            if (message.SeriesId.HasValue)
            {
                var seriesId = message.SeriesId.Value;
                pagingSpec.FilterExpressions.Add(v => v.SeriesId == seriesId);
            }

            if (monitored)
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Series.Monitored == true);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Series.Monitored == false);
            }

            var episodes = _episodeCutoffService.EpisodesWhereCutoffUnmet(pagingSpec).Records.ToList();
            var queue = _queueService.GetQueue().Where(q => q.Episode != null).Select(q => q.Episode.Id);
            var cutoffUnmet = episodes.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForBulkEpisodes(cutoffUnmet, monitored, message.Trigger == CommandTrigger.Manual).GetAwaiter().GetResult();
        }
    }
}
