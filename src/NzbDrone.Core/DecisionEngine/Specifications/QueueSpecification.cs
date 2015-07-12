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
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                       UpgradableSpecification UpgradableSpecification,
                                       Logger logger)
        {
            _queueService = queueService;
            _upgradableSpecification = UpgradableSpecification;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue()
                            .Select(q => q.RemoteEpisode).ToList();

            var matchingSeries = queue.Where(q => q.Series.Id == subject.Series.Id);
            var matchingEpisode = matchingSeries.Where(q => q.Episodes.Select(e => e.Id).Intersect(subject.Episodes.Select(e => e.Id)).Any());

            foreach (var remoteEpisode in matchingEpisode)
            {
                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);

                if (!_upgradableSpecification.CutoffNotMet(subject.Series.Profile, 
                                                           subject.Series.LanguageProfile, 
                                                           remoteEpisode.ParsedEpisodeInfo.Quality, 
                                                           remoteEpisode.ParsedEpisodeInfo.Language, 
                                                           subject.ParsedEpisodeInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue already meets cutoff: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);

                if (!_upgradableSpecification.IsUpgradable(subject.Series.Profile,
                                                           subject.Series.LanguageProfile, 
                                                           remoteEpisode.ParsedEpisodeInfo.Quality, 
                                                           remoteEpisode.ParsedEpisodeInfo.Language, 
                                                           subject.ParsedEpisodeInfo.Quality, 
                                                           subject.ParsedEpisodeInfo.Language))
                {
                    return Decision.Reject("Quality for release in queue is of equal or higher preference: {0} - {1}", remoteEpisode.ParsedEpisodeInfo.Quality, remoteEpisode.ParsedEpisodeInfo.Language);
                }
            }

            return Decision.Accept();
        }
    }
}
