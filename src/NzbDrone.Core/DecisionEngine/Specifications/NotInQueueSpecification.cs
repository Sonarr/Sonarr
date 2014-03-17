using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotInQueueSpecification : IDecisionEngineSpecification
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly Logger _logger;

        public NotInQueueSpecification(IProvideDownloadClient downloadClientProvider, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Already in download queue.";
            }
        }

        public bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (downloadClient == null)
            {
                _logger.Warn("Download client isn't configured yet.");
                return true;
            }

            var queue = downloadClient.GetQueue().Select(q => q.RemoteEpisode);

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
            var matchingSeriesAndQuality = matchingSeries.Where(q => new QualityModelComparer(q.Series.QualityProfile).Compare(q.ParsedEpisodeInfo.Quality, newEpisode.ParsedEpisodeInfo.Quality) >= 0);

            return matchingSeriesAndQuality.Any(q => q.Episodes.Select(e => e.Id).Intersect(newEpisode.Episodes.Select(e => e.Id)).Any());
        }
    }
}
