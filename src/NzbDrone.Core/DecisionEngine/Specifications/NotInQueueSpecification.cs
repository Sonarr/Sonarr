using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotInQueueSpecification : IDecisionEngineSpecification
    {
        private readonly IDownloadTrackingService _downloadTrackingService;
        private readonly Logger _logger;

        public NotInQueueSpecification(IDownloadTrackingService downloadTrackingService, Logger logger)
        {
            _downloadTrackingService = downloadTrackingService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Already in download queue.";
            }
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _downloadTrackingService.GetQueuedDownloads()
                            .Where(v => v.State == TrackedDownloadState.Downloading)
                            .Select(q => q.RemoteEpisode).ToList();

            if (IsInQueue(subject, queue))
            {
                _logger.Debug("Already in queue, rejecting.");
                return false;
            }

            return true;
        }

        private bool IsInQueue(RemoteEpisode newEpisode, IEnumerable<RemoteEpisode> queue)
        {
            var matchingSeries = queue.Where(q => q.Series.Id == newEpisode.Series.Id);
            var matchingSeriesAndQuality = matchingSeries.Where(q => new QualityModelComparer(q.Series.Profile).Compare(q.ParsedEpisodeInfo.Quality, newEpisode.ParsedEpisodeInfo.Quality) >= 0);

            return matchingSeriesAndQuality.Any(q => q.Episodes.Select(e => e.Id).Intersect(newEpisode.Episodes.Select(e => e.Id)).Any());
        }
    }
}
