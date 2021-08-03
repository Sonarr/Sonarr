using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SceneMappingSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SceneMappingSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Temporary; // Temporary till there's a mapping

        public Decision IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria)
        {
            if (remoteEpisode.SceneMapping == null)
            {
                _logger.Debug("No applicable scene mapping, skipping.");
                return Decision.Accept();
            }

            if (remoteEpisode.SceneMapping.SceneOrigin.IsNullOrWhiteSpace())
            {
                _logger.Debug("No explicit scene origin in scene mapping.");
                return Decision.Accept();
            }

            var split = remoteEpisode.SceneMapping.SceneOrigin.Split(':');

            var isInteractive = searchCriteria != null && searchCriteria.InteractiveSearch;

            if (remoteEpisode.SceneMapping.Comment.IsNotNullOrWhiteSpace())
            {
                _logger.Debug("SceneMapping has origin {0} with comment '{1}'.", remoteEpisode.SceneMapping.SceneOrigin, remoteEpisode.SceneMapping.Comment);
            }
            else
            {
                _logger.Debug("SceneMapping has origin {0}.", remoteEpisode.SceneMapping.SceneOrigin);
            }

            if (split[0] == "mixed")
            {
                _logger.Debug("SceneMapping origin is explicitly mixed, this means these were released with multiple unidentifiable numbering schemes.");

                if (remoteEpisode.SceneMapping.Comment.IsNotNullOrWhiteSpace())
                {
                    return Decision.Reject("{0} has ambiguous numbering");
                }
                else
                {
                    return Decision.Reject("Ambiguous numbering");
                }
            }

            if (split[0] == "unknown")
            {
                var type = split.Length >= 2 ? split[1] : "scene";

                _logger.Debug("SceneMapping origin is explicitly unknown, unsure what numbering scheme it uses but '{0}' will be assumed. Provide full release title to Sonarr/TheXEM team.", type);
            }

            return Decision.Accept();
        }
    }
}
