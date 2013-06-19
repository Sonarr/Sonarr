using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.Download
{
    public interface IDownloadApprovedReportsService
    {
        List<DownloadDecision> DownloadApproved(List<DownloadDecision> decisions);
    }

    public class DownloadApprovedReportsService : IDownloadApprovedReportsService
    {
        private readonly IDownloadService _downloadService;
        private readonly Logger _logger;

        public DownloadApprovedReportsService(IDownloadService downloadService, Logger logger)
        {
            _downloadService = downloadService;
            _logger = logger;
        }

        public List<DownloadDecision> DownloadApproved(List<DownloadDecision> decisions)
        {
            var qualifiedReports = decisions
                         .Where(c => c.Approved)
                         .ToList();

            var remoteEpisodes = qualifiedReports
                         .Select(c => c.RemoteEpisode)
                         .OrderByDescending(c => c.ParsedEpisodeInfo.Quality)
                         .ThenBy(c => c.Episodes.Select(e => e.EpisodeNumber).MinOrDefault())
                         .ThenBy(c => c.Report.Age)
                         .ToList();

            var downloadedReports = new List<int>();

            foreach (var episodeParseResult in remoteEpisodes)
            {
                try
                {
                    if (downloadedReports.Intersect(episodeParseResult.Episodes.Select(e => e.Id)).Any()) continue;

                    _downloadService.DownloadReport(episodeParseResult);
                    downloadedReports.AddRange(episodeParseResult.Episodes.Select(e => e.Id));
                }
                catch (Exception e)
                {
                    _logger.WarnException("Couldn't add report to download queue. " + episodeParseResult, e);
                }
            }

            return qualifiedReports;
        }
    }
}
