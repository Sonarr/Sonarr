using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Pending;
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
            var failed = new List<DownloadDecision>();

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
                    _pendingReleaseService.Add(report, PendingReleaseReason.Delay);
                    pending.Add(report);
                    continue;
                }

                if (downloadProtocol == DownloadProtocol.Usenet && usenetFailed ||
                    downloadProtocol == DownloadProtocol.Torrent && torrentFailed)
                {
                    failed.Add(report);
                    continue;
                }

                try
                {
                    _downloadService.DownloadReport(remoteEpisode);
                    grabbed.Add(report);
                }
                catch (Exception ex)
                {
                    if (ex is DownloadClientUnavailableException || ex is DownloadClientAuthenticationException)
                    {
                        _logger.Debug(ex, "Failed to send release to download client, storing until later. " + remoteEpisode);
                        failed.Add(report);

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

            pending.AddRange(ProcessFailedGrabs(grabbed, failed));

            return new ProcessedDecisions(grabbed, pending, decisions.Where(d => d.Rejected).ToList());
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

        private List<DownloadDecision> ProcessFailedGrabs(List<DownloadDecision> grabbed, List<DownloadDecision> failed)
        {
            var pending = new List<DownloadDecision>();
            var stored = new List<DownloadDecision>();

            foreach (var report in failed)
            {
                // If a release was already grabbed with matching episodes we should store it as a fallback
                // and filter it out the next time it is processed incase a higher quality release failed to
                // add to the download client, but a lower quality release was sent to another client
                // If the release wasn't grabbed already, but was already stored, store it as a fallback,
                // otherwise store it as DownloadClientUnavailable.

                if (IsEpisodeProcessed(grabbed, report))
                {
                    _pendingReleaseService.Add(report, PendingReleaseReason.Fallback);
                    pending.Add(report);
                }
                else if (IsEpisodeProcessed(stored, report))
                {
                    _pendingReleaseService.Add(report, PendingReleaseReason.Fallback);
                    pending.Add(report);
                }
                else
                {
                    _pendingReleaseService.Add(report, PendingReleaseReason.DownloadClientUnavailable);
                    pending.Add(report);
                    stored.Add(report);
                }
            }

            return pending;
        }
    }
}
