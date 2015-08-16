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
            var grabbed = new List<DownloadDecision>();
            var pending = new List<DownloadDecision>();

            foreach (var report in prioritizedDecisions)
            {
                var remoteEpisode = report.RemoteItem as RemoteEpisode;
                var remoteMovie = report.RemoteItem as RemoteMovie;

                // Process series
                if (remoteEpisode != null)
                {

                    var episodeIds = remoteEpisode.Episodes.Select(e => e.Id).ToList();

                    //Skip if already grabbed
                    if (grabbed.Where(r => r.RemoteItem != null && r.RemoteItem is RemoteEpisode)
                                    .SelectMany(r => (r.RemoteItem as RemoteEpisode).Episodes)
                                    .Select(e => e.Id)
                                    .ToList()
                                    .Intersect(episodeIds)
                                    .Any())
                    {
                        continue;
                    }

                    if (report.TemporarilyRejected)
                    {
                        _pendingReleaseService.Add(report);
                        pending.Add(report);
                        continue;
                    }

                    if (pending.Where(r => r.RemoteItem != null && r.RemoteItem is RemoteEpisode)
                            .SelectMany(r => (r.RemoteItem as RemoteEpisode).Episodes)
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
                    catch (Exception e)
                    {
                        //TODO: support for store & forward
                        //We'll need to differentiate between a download client error and an indexer error
                        _logger.WarnException("Couldn't add report to download queue. " + remoteEpisode, e);
                    }
                }

                if (remoteMovie != null)
                {
                    var movieId = remoteMovie.Movie.Id;

                    //Skip if already grabbed
                    if (grabbed.Where(r => r.RemoteItem != null && r.RemoteItem is RemoteMovie)
                               .Any(r => (r.RemoteItem as RemoteMovie).Movie.Id == movieId))
                    {
                        continue;
                    }

                    if (report.TemporarilyRejected)
                    {
                        _pendingReleaseService.Add(report);
                        pending.Add(report);
                        continue;
                    }

                    if (pending.Where(r => r.RemoteItem != null && r.RemoteItem is RemoteMovie)
                               .Any(r => (r.RemoteItem as RemoteMovie).Movie.Id == movieId))
                    {
                        continue;
                    }

                    try
                    {
                        _downloadService.DownloadReport(remoteMovie);
                        grabbed.Add(report);
                    }
                    catch (Exception e)
                    {
                        //TODO: support for store & forward
                        //We'll need to differentiate between a download client error and an indexer error
                        _logger.WarnException("Couldn't add report to download queue. " + remoteEpisode, e);
                    }

                }
            }

            return new ProcessedDecisions(grabbed, pending, decisions.Where(d => d.Rejected).ToList());
        }

        internal List<DownloadDecision> GetQualifiedReports(IEnumerable<DownloadDecision> decisions)
        {
            //Process both approved and temporarily rejected
            return decisions.Where(c => (c.Approved || c.TemporarilyRejected) && 
                                  (c.RemoteItem != null && ((c.RemoteItem is RemoteMovie) || (c.RemoteItem as RemoteEpisode).Episodes.Any())))
                                  .ToList();
        }
    }
}
