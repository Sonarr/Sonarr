using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class EpisodeRequestedSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public EpisodeRequestedSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
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

                if (remoteEpisodes.Any())
                {
                    var episodes = remoteEpisode.Episodes.OrderBy(v => v.SeasonNumber).ThenBy(v => v.EpisodeNumber).ToList();

                    if (episodes.Count > 1)
                    {
                        return Decision.Reject($"Episode wasn't requested: {episodes.First().SeasonNumber}x{episodes.First().EpisodeNumber}-{episodes.Last().EpisodeNumber}");
                    }
                    else
                    {
                        return Decision.Reject($"Episode wasn't requested: {episodes.First().SeasonNumber}x{episodes.First().EpisodeNumber}");
                    }
                }
                else
                {
                    return Decision.Reject("Episode wasn't requested");
                }
            }

            return Decision.Accept();
        }
    }
}
