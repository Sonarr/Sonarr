using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QueueSpecification : IDecisionEngineSpecification
    {
        private readonly IQueueService _queueService;
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                       QualityUpgradableSpecification qualityUpgradableSpecification,
                                       Logger logger)
        {
            _queueService = queueService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue()
                            .Select(q => q.RemoteEpisode).ToList();

            var matchingSeries = queue.Where(q => q.Series.Id == subject.Series.Id);
            var matchingEpisode = matchingSeries.Where(q => q.Episodes.Select(e => e.Id).Intersect(subject.Episodes.Select(e => e.Id)).Any());

            foreach (var remoteEpisode in matchingEpisode)
            {
                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);

                if (!_qualityUpgradableSpecification.CutoffNotMet(subject.Series.Profile, remoteEpisode.ParsedEpisodeInfo.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue already meets cutoff: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);

                if (!_qualityUpgradableSpecification.IsUpgradable(subject.Series.Profile, remoteEpisode.ParsedEpisodeInfo.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue is of equal or higher preference: {0}", remoteEpisode.ParsedEpisodeInfo.Quality);
                }
            }

            return Decision.Accept();
        }
    }
}
