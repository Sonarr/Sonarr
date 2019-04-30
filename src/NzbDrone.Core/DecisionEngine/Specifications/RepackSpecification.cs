using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RepackSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public RepackSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (!subject.ParsedEpisodeInfo.Quality.Revision.IsRepack)
            {
                return Decision.Accept();
            }

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                var releaseGroup = subject.ParsedEpisodeInfo.ReleaseGroup;
                var fileReleaseGroup = file.ReleaseGroup;

                if (!fileReleaseGroup.Equals(releaseGroup, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Release is a repack for a different release group. Release Group: {0}. File release group: {0}", releaseGroup, fileReleaseGroup);
                    return Decision.Reject("Release is a repack for a different release group. Release Group: {0}. File release group: {0}", releaseGroup, fileReleaseGroup);
                }
            }

            return Decision.Accept();
        }
    }
}
