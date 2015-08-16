using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;


namespace NzbDrone.Core.DecisionEngine.Specifications.Search.Series
{
    public class EpisodeRequestedSpecification : BaseSeriesSpecification
    {
        private readonly Logger _logger;

        public EpisodeRequestedSpecification(Logger logger)
        {
            _logger = logger;
        }

        public override RejectionType Type { get { return RejectionType.Permanent; } }

        public override Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SeriesSearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            var criteriaEpisodes = searchCriteria.Episodes.Select(v => v.Id).ToList();
            var remoteEpisodes = remoteEpisode.Episodes.Select(v => v.Id).ToList();

            if (!criteriaEpisodes.Intersect(remoteEpisodes).Any())
            {
                _logger.Debug("Release rejected since the episode wasn't requested: {0}", remoteEpisode.ParsedEpisodeInfo);
                return Decision.Reject("Episode wasn't requested");
            }

            return Decision.Accept();
        }
    }
}
