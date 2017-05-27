using System;
using System.Collections.Generic;
using System.Linq;
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
            var storedUsenet = new List<DownloadDecision>();
            var fallbackUsenet = new List<DownloadDecision>();
            var storedTorrent = new List<DownloadDecision>();
            var fallbackTorrent = new List<DownloadDecision>();

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

                if (IsEpisodeProcessed(pending, report))
                {
                    continue;
                }

                if (downloadProtocol == DownloadProtocol.Usenet)
                {
                    if (IsEpisodeProcessed(storedUsenet, report))
                    {
                        fallbackUsenet.Add(report);
                        continue;
                    }
                    else if (storedUsenet.Any())
                    {
                        storedUsenet.Add(report);
                        continue;
                    }
                }

                if (downloadProtocol == DownloadProtocol.Torrent)
                {
                    if (IsEpisodeProcessed(storedTorrent, report))
                    {
                        fallbackTorrent.Add(report);
                        continue;
                    }
                    else if (storedTorrent.Any())
                    {
                        storedTorrent.Add(report);
                        continue;
                    }
                }

                try
                {
                    _downloadService.DownloadReport(remoteEpisode);
                    grabbed.Add(report);
                }
                catch (DownloadClientUnavailableException e)
                {
                    _logger.Debug("Failed to send release to download client, storing until later");

                    if (downloadProtocol == DownloadProtocol.Torrent)
                    {
                        storedTorrent.Add(report);
                    }
                    else
                    {
                        storedUsenet.Add(report);
                    }
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't add report to download queue. " + remoteEpisode);
                }
            }

            pending.AddRange(ProcessFailedGrabs(grabbed, storedUsenet, PendingReleaseReason.DownloadClientUnavailable));
            pending.AddRange(ProcessFailedGrabs(grabbed, fallbackUsenet, PendingReleaseReason.Fallback));
            pending.AddRange(ProcessFailedGrabs(grabbed, storedTorrent, PendingReleaseReason.DownloadClientUnavailable));
            pending.AddRange(ProcessFailedGrabs(grabbed, fallbackTorrent, PendingReleaseReason.Fallback));

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

        private List<DownloadDecision> ProcessFailedGrabs(List<DownloadDecision> grabbed, List<DownloadDecision> failed, PendingReleaseReason reason)
        {
            var pending = new List<DownloadDecision>();

            foreach (var report in failed)
            {
                if (!IsEpisodeProcessed(grabbed, report))
                {
                    _pendingReleaseService.Add(report, reason);
                    pending.Add(report);
                }
            }

            return pending;
        }
    }
}
