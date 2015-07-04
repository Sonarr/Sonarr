using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public class RssSyncService : IExecute<RssSyncCommand>
    {
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IIndexerFactory _indexerFactory;
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IEpisodeSearchService _episodeSearchService;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RssSyncService(IIndexerStatusService indexerStatusService,
                              IIndexerFactory indexerFactory,
                              IFetchAndParseRss rssFetcherAndParser,
                              IMakeDownloadDecision downloadDecisionMaker,
                              IProcessDownloadDecisions processDownloadDecisions,
                              IEpisodeSearchService episodeSearchService,
                              IPendingReleaseService pendingReleaseService,
                              IEventAggregator eventAggregator,
                              Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _indexerFactory = indexerFactory;
            _rssFetcherAndParser = rssFetcherAndParser;
            _downloadDecisionMaker = downloadDecisionMaker;
            _processDownloadDecisions = processDownloadDecisions;
            _episodeSearchService = episodeSearchService;
            _pendingReleaseService = pendingReleaseService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }


        private ProcessedDecisions Sync()
        {
            _logger.ProgressInfo("Starting RSS Sync");

            var reports = _rssFetcherAndParser.Fetch().Concat(FilterByIndexer(_pendingReleaseService.GetPending())).ToList();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var processed = _processDownloadDecisions.ProcessDecisions(decisions);

            var message = String.Format("RSS Sync Completed. Reports found: {0}, Reports grabbed: {1}", reports.Count, processed.Grabbed.Count);

            if (processed.Pending.Any())
            {
                message += ", Reports pending: " + processed.Pending.Count;
            }

            _logger.ProgressInfo(message);

            return processed;
        }

        private IEnumerable<ReleaseInfo> FilterByIndexer(IEnumerable<ReleaseInfo> releases)
        {
            var indexerBackOff = new Dictionary<int, bool>();

            foreach (var release in releases)
            {
                bool ignore;
                if (!indexerBackOff.TryGetValue(release.IndexerId, out ignore))
                {
                    var indexerStatus = _indexerStatusService.GetIndexerStatus(release.IndexerId);

                    if (indexerStatus == null || !indexerStatus.DisabledTill.HasValue || indexerStatus.DisabledTill.Value < DateTime.UtcNow)
                    {
                        indexerBackOff[release.IndexerId] = ignore = false;
                    }
                    else
                    {
                        indexerBackOff[release.IndexerId] = ignore = true;
                    }
                }

                if (!ignore)
                {
                    yield return release;
                }
            }
        }

        public void Execute(RssSyncCommand message)
        {
            var rssStarted = DateTime.UtcNow;

            var processed = Sync();
            var grabbedOrPending = processed.Grabbed.Concat(processed.Pending).ToList();

            if (message.LastExecutionTime.HasValue && DateTime.UtcNow.Subtract(message.LastExecutionTime.Value).TotalHours > 3)
            {
                var lastGap = rssStarted;

                foreach (var indexer in _indexerFactory.RssEnabled().Where(v => v.SupportsSearch))
                {
                    var indexerStatus = _indexerStatusService.GetIndexerStatus(indexer.Definition.Id);
                    if (indexerStatus == null || !indexerStatus.LastContinuousRssSync.HasValue || indexerStatus.LastContinuousRssSync.Value >= rssStarted)
                    {
                        continue;
                    }
                    var lastRecentSearch = indexerStatus.LastContinuousRssSync.Value;

                    if (indexerStatus.DisabledTill.HasValue && indexerStatus.DisabledTill.Value > rssStarted)
                    {
                        _logger.Debug("Indexer {0} last continous rss sync was till {1}. But indexer is temporarily disabled, unable to search for missing episodes.", indexer.Definition.Name, lastRecentSearch);
                        continue;
                    }

                    _logger.Debug("Indexer {0} last continous rss sync was till {1}. Search may be required.", indexer.Definition.Name, lastRecentSearch);

                    if (lastRecentSearch < lastGap)
                    {
                        lastGap = lastRecentSearch;
                    }
                }

                _logger.Info("RSS Sync hasn't run since {0} and didn't completely cover the period starting {1}. Searching for any missing episodes aired within that period.", message.LastExecutionTime.Value, lastGap);
                _episodeSearchService.MissingEpisodesAiredAfter(lastGap, grabbedOrPending.SelectMany(d => d.RemoteEpisode.Episodes).Select(e => e.Id));
            }

            _eventAggregator.PublishEvent(new RssSyncCompleteEvent(processed));
        }
    }
}
