using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser.Model;

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
            var downloadedReports = new List<DownloadDecision>();
            var pendingReports = new List<DownloadDecision>();

            foreach (var report in prioritizedDecisions)
            {
                var remoteEpisode = report.RemoteEpisode;

                if (DownloadingOrPending(downloadedReports, pendingReports, remoteEpisode))
                {
                    continue;
                }

                if (report.TemporarilyRejected)
                {
                    _pendingReleaseService.Add(report);
                    pendingReports.Add(report);
                    continue;
                }

                try
                {
                    _downloadService.DownloadReport(remoteEpisode);
                    downloadedReports.Add(report);
                }
                catch (Exception e)
                {
                    //TODO: support for store & forward
                    _logger.WarnException("Couldn't add report to download queue. " + remoteEpisode, e);
                }
            }

            return new ProcessedDecisions(downloadedReports, pendingReports);
        }

        internal List<DownloadDecision> GetQualifiedReports(IEnumerable<DownloadDecision> decisions)
        {
            //Process both approved and temporarily rejected
            return decisions.Where(c => (c.Approved || c.TemporarilyRejected) && c.RemoteEpisode.Episodes.Any()).ToList();
        }

        private bool DownloadingOrPending(List<DownloadDecision> downloading, List<DownloadDecision> pending, RemoteEpisode remoteEpisode)
        {
            var episodeIds = remoteEpisode.Episodes.Select(e => e.Id).ToList();

            if (downloading.SelectMany(r => r.RemoteEpisode.Episodes)
                           .Select(e => e.Id)
                           .ToList()
                           .Intersect(episodeIds)
                           .Any())
            {
                return true;
            }

            if (pending.SelectMany(r => r.RemoteEpisode.Episodes)
                       .Select(e => e.Id)
                       .ToList()
                       .Intersect(episodeIds)
                       .Any())
            {
                return true;
            }

            return false;
        }
    }
}
