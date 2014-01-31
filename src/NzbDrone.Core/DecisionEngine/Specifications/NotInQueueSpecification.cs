using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

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

            if (!downloadClient.IsConfigured)
            {
                _logger.Warn("Download client {0} isn't configured yet.", downloadClient.GetType().Name);
                return true;
            }

            var queue = downloadClient.GetQueue().Select(q => q.RemoteEpisode);

            return !IsInQueue(subject, queue);
        }

        private bool IsInQueue(RemoteEpisode newEpisode, IEnumerable<RemoteEpisode> queue)
        {
            var matchingSeries = queue.Where(q => q.Series.Id == newEpisode.Series.Id);
            var matchingSeriesAndQuality = matchingSeries.Where(q => new QualityModelComparer(q.Series.QualityProfile).Compare(q.ParsedEpisodeInfo.Quality, newEpisode.ParsedEpisodeInfo.Quality) >= 0);

            return matchingSeriesAndQuality.Any(q => q.Episodes.Select(e => e.Id).Intersect(newEpisode.Episodes.Select(e => e.Id)).Any());
        }
    }
}
