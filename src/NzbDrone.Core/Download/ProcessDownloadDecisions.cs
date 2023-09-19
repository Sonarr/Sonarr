using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Download
{
    public interface IProcessDownloadDecisions
    {
        Task<ProcessedDecisions> ProcessDecisions(List<DownloadDecision> decisions);
        Task<ProcessedDecisionResult> ProcessDecision(DownloadDecision decision, int? downloadClientId);
    }

    public class ProcessDownloadDecisions : IProcessDownloadDecisions
    {
        private readonly IDownloadService _downloadService;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly Logger _logger;

        public ProcessDownloadDecisions(IDownloadService downloadService,
                                        IPrioritizeDownloadDecision prioritizeDownloadDecision,
                                        IPendingReleaseService pendingReleaseService,
                                        Logger logger)
        {
            _downloadService = downloadService;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _pendingReleaseService = pendingReleaseService;
            _logger = logger;
        }

        public async Task<ProcessedDecisions> ProcessDecisions(List<DownloadDecision> decisions)
        {
            var qualifiedReports = GetQualifiedReports(decisions);
            var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(qualifiedReports);
            var grabbed = new List<DownloadDecision>();
            var pending = new List<DownloadDecision>();
            var rejected = decisions.Where(d => d.Rejected).ToList();

            var pendingAddQueue = new List<Tuple<DownloadDecision, PendingReleaseReason>>();

            var usenetFailed = false;
            var torrentFailed = false;

            foreach (var report in prioritizedDecisions)
            {
                var downloadProtocol = report.RemoteEpisode.Release.DownloadProtocol;

                // Skip if already grabbed
                if (IsEpisodeProcessed(grabbed, report))
                {
                    continue;
                }

                if (report.TemporarilyRejected)
                {
                    PreparePending(pendingAddQueue, grabbed, pending, report, PendingReleaseReason.Delay);
                    continue;
                }

                if ((downloadProtocol == DownloadProtocol.Usenet && usenetFailed) ||
                    (downloadProtocol == DownloadProtocol.Torrent && torrentFailed))
                {
                    PreparePending(pendingAddQueue, grabbed, pending, report, PendingReleaseReason.DownloadClientUnavailable);
                    continue;
                }

                var result = await ProcessDecisionInternal(report);

                switch (result)
                {
                    case ProcessedDecisionResult.Grabbed:
                        {
                            grabbed.Add(report);
                            break;
                        }

                    case ProcessedDecisionResult.Pending:
                        {
                            PreparePending(pendingAddQueue, grabbed, pending, report, PendingReleaseReason.Delay);
                            break;
                        }

                    case ProcessedDecisionResult.Rejected:
                        {
                            rejected.Add(report);
                            break;
                        }

                    case ProcessedDecisionResult.Failed:
                        {
                            PreparePending(pendingAddQueue, grabbed, pending, report, PendingReleaseReason.DownloadClientUnavailable);

                            if (downloadProtocol == DownloadProtocol.Usenet)
                            {
                                usenetFailed = true;
                            }
                            else if (downloadProtocol == DownloadProtocol.Torrent)
                            {
                                torrentFailed = true;
                            }

                            break;
                        }

                    case ProcessedDecisionResult.Skipped:
                        {
                            break;
                        }
                }
            }

            if (pendingAddQueue.Any())
            {
                _pendingReleaseService.AddMany(pendingAddQueue);
            }

            return new ProcessedDecisions(grabbed, pending, rejected);
        }

        public async Task<ProcessedDecisionResult> ProcessDecision(DownloadDecision decision, int? downloadClientId)
        {
            if (decision == null)
            {
                return ProcessedDecisionResult.Skipped;
            }

            if (!IsQualifiedReport(decision))
            {
                return ProcessedDecisionResult.Rejected;
            }

            if (decision.TemporarilyRejected)
            {
                _pendingReleaseService.Add(decision, PendingReleaseReason.Delay);

                return ProcessedDecisionResult.Pending;
            }

            var result = await ProcessDecisionInternal(decision, downloadClientId);

            if (result == ProcessedDecisionResult.Failed)
            {
                _pendingReleaseService.Add(decision, PendingReleaseReason.DownloadClientUnavailable);
            }

            return result;
        }

        internal List<DownloadDecision> GetQualifiedReports(IEnumerable<DownloadDecision> decisions)
        {
            return decisions.Where(IsQualifiedReport).ToList();
        }

        internal bool IsQualifiedReport(DownloadDecision decision)
        {
            // Process both approved and temporarily rejected
            return (decision.Approved || decision.TemporarilyRejected) && decision.RemoteEpisode.Episodes.Any();
        }

        private bool IsEpisodeProcessed(List<DownloadDecision> decisions, DownloadDecision report)
        {
            var episodeIds = report.RemoteEpisode.Episodes.Select(e => e.Id).ToList();

            return decisions.SelectMany(r => r.RemoteEpisode.Episodes)
                            .Select(e => e.Id)
                            .ToList()
                            .Intersect(episodeIds)
                            .Any();
        }

        private void PreparePending(List<Tuple<DownloadDecision, PendingReleaseReason>> queue, List<DownloadDecision> grabbed, List<DownloadDecision> pending, DownloadDecision report, PendingReleaseReason reason)
        {
            // If a release was already grabbed with matching episodes we should store it as a fallback
            // and filter it out the next time it is processed.
            // If a higher quality release failed to add to the download client, but a lower quality release
            // was sent to another client we still list it normally so it apparent that it'll grab next time.
            // Delayed is treated the same, but only the first is listed the subsequent items as stored as Fallback.

            if (IsEpisodeProcessed(grabbed, report) ||
                IsEpisodeProcessed(pending, report))
            {
                reason = PendingReleaseReason.Fallback;
            }

            queue.Add(Tuple.Create(report, reason));
            pending.Add(report);
        }

        private async Task<ProcessedDecisionResult> ProcessDecisionInternal(DownloadDecision decision, int? downloadClientId = null)
        {
            var remoteEpisode = decision.RemoteEpisode;

            try
            {
                _logger.Trace("Grabbing from Indexer {0} at priority {1}.", remoteEpisode.Release.Indexer, remoteEpisode.Release.IndexerPriority);
                await _downloadService.DownloadReport(remoteEpisode, downloadClientId);

                return ProcessedDecisionResult.Grabbed;
            }
            catch (ReleaseUnavailableException)
            {
                _logger.Warn("Failed to download release from indexer, no longer available. " + remoteEpisode);
                return ProcessedDecisionResult.Rejected;
            }
            catch (Exception ex)
            {
                if (ex is DownloadClientUnavailableException || ex is DownloadClientAuthenticationException)
                {
                    _logger.Debug(ex,
                        "Failed to send release to download client, storing until later. " + remoteEpisode);

                    return ProcessedDecisionResult.Failed;
                }
                else
                {
                    _logger.Warn(ex, "Couldn't add report to download queue. " + remoteEpisode);
                    return ProcessedDecisionResult.Skipped;
                }
            }
        }
    }
}
