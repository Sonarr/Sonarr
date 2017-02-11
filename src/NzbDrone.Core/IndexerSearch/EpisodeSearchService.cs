using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IEpisodeService _episodeService;
        private readonly IEpisodeCutoffService _episodeCutoffService;
        private readonly IQueueService _queueService;
        private readonly Logger _logger;

        public EpisodeSearchService(ISearchForNzb nzbSearchService,
                                    IProcessDownloadDecisions processDownloadDecisions,
                                    IEpisodeService episodeService,
                                    IEpisodeCutoffService episodeCutoffService,
                                    IQueueService queueService,
                                    Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _episodeService = episodeService;
            _episodeCutoffService = episodeCutoffService;
            _queueService = queueService;
            _logger = logger;
        }

        private void SearchForMissingEpisodes(List<Episode> episodes, bool userInvokedSearch)
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
                        try
                        {
                            decisions = _nzbSearchService.SeasonSearch(series.Key, season.Key, true, userInvokedSearch);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Unable to search for missing episodes in season {0} of [{1}]", season.Key, series.Key);
                            continue;
                        }
                    }

                    else
                    {
                        try
                        {
                            decisions = _nzbSearchService.EpisodeSearch(season.First(), userInvokedSearch);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Unable to search for missing episode: [{0}]", season.First());
                            continue;
                        }
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
                var decisions = _nzbSearchService.EpisodeSearch(episodeId, message.Trigger == CommandTrigger.Manual);
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
                var pagingSpec = new PagingSpec<Episode>
                                 {
                                     Page = 1,
                                     PageSize = 100000,
                                     SortDirection = SortDirection.Ascending,
                                     SortKey = "Id"
                                 };

                pagingSpec.FilterExpressions.Add(v => v.Monitored == true &&v.Series.Monitored == true);

                episodes = _episodeService.EpisodesWithoutFiles(pagingSpec).Records.ToList();
            }

            var queue = _queueService.GetQueue().Select(q => q.Episode.Id);
            var missing = episodes.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForMissingEpisodes(missing, message.Trigger == CommandTrigger.Manual);
        }

        public void Execute(CutoffUnmetEpisodeSearchCommand message)
        {
            Expression<Func<Episode, bool>> filterExpression;

            if (message.SeriesId.HasValue)
            {
                filterExpression = v =>
                                   v.SeriesId == message.SeriesId.Value &&
                                   v.Monitored == true &&
                                   v.Series.Monitored == true;
            }

            else
            {
                filterExpression = v =>
                                   v.Monitored == true &&
                                   v.Series.Monitored == true;
            }

            var pagingSpec = new PagingSpec<Episode>
                             {
                                 Page = 1,
                                 PageSize = 100000,
                                 SortDirection = SortDirection.Ascending,
                                 SortKey = "Id"
                             };

            pagingSpec.FilterExpressions.Add(filterExpression);

            var episodes = _episodeCutoffService.EpisodesWhereCutoffUnmet(pagingSpec).Records.ToList();

            var queue = _queueService.GetQueue().Select(q => q.Episode.Id);
            var missing = episodes.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForMissingEpisodes(missing, message.Trigger == CommandTrigger.Manual);
        }
    }
}
