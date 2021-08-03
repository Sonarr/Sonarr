using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        ProcessedDecisions ProcessDecisions(List<DownloadDecision> decisions);
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

        public ProcessedDecisions ProcessDecisions(List<DownloadDecision> decisions)
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
                var remoteEpisode = report.RemoteEpisode;
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

                try
                {
                    _logger.Trace("Grabbing from Indexer {0} at priority {1}.", remoteEpisode.Release.Indexer, remoteEpisode.Release.IndexerPriority);
                    _downloadService.DownloadReport(remoteEpisode);
                    grabbed.Add(report);
                }
                catch (ReleaseUnavailableException)
                {
                    _logger.Warn("Failed to download release from indexer, no longer available. " + remoteEpisode);
                    rejected.Add(report);
                }
                catch (Exception ex)
                {
                    if (ex is DownloadClientUnavailableException || ex is DownloadClientAuthenticationException)
                    {
                        _logger.Debug(ex, "Failed to send release to download client, storing until later. " + remoteEpisode);
                        PreparePending(pendingAddQueue, grabbed, pending, report, PendingReleaseReason.DownloadClientUnavailable);

                        if (downloadProtocol == DownloadProtocol.Usenet)
                        {
                            usenetFailed = true;
                        }
                        else if (downloadProtocol == DownloadProtocol.Torrent)
                        {
                            torrentFailed = true;
                        }
                    }
                    else
                    {
                        _logger.Warn(ex, "Couldn't add report to download queue. " + remoteEpisode);
                    }
                }
            }

            if (pendingAddQueue.Any())
            {
                _pendingReleaseService.AddMany(pendingAddQueue);
            }

            return new ProcessedDecisions(grabbed, pending, rejected);
        }

        internal List<DownloadDecision> GetQualifiedReports(IEnumerable<DownloadDecision> decisions)
        {
            //Process both approved and temporarily rejected
            return decisions.Where(c => (c.Approved || c.TemporarilyRejected) && c.RemoteEpisode.Episodes.Any()).ToList();
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
    }
}
