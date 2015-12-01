using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Pending;

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

            foreach (var report in prioritizedDecisions)
            {
                var remoteEpisode = report.RemoteEpisode;

                var episodeIds = remoteEpisode.Episodes.Select(e => e.Id).ToList();

                //Skip if already grabbed
                if (grabbed.SelectMany(r => r.RemoteEpisode.Episodes)
                                .Select(e => e.Id)
                                .ToList()
                                .Intersect(episodeIds)
                                .Any())
                {
                    continue;
                }

                if (report.TemporarilyRejected)
                {
                    _pendingReleaseService.Add(report, PendingReleaseReason.Delay);
                    pending.Add(report);
                    continue;
                }

                if (pending.SelectMany(r => r.RemoteEpisode.Episodes)
                        .Select(e => e.Id)
                        .ToList()
                        .Intersect(episodeIds)
                        .Any())
                {
                    continue;
                }

                try
                {
                    _downloadService.DownloadReport(remoteEpisode);
                    grabbed.Add(report);
                }
                catch (DownloadClientUnavailableException e)
                {
                    _logger.Debug("Failed to send release to download client, storing until later");
                    _pendingReleaseService.Add(report, PendingReleaseReason.StoreAndForward);
                    pending.Add(report);
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't add report to download queue. " + remoteEpisode);
                }
            }

            return new ProcessedDecisions(grabbed, pending, decisions.Where(d => d.Rejected).ToList());
        }

        internal List<DownloadDecision> GetQualifiedReports(IEnumerable<DownloadDecision> decisions)
        {
            //Process both approved and temporarily rejected
            return decisions.Where(c => (c.Approved || c.TemporarilyRejected) && c.RemoteEpisode.Episodes.Any()).ToList();
        }
    }
}
