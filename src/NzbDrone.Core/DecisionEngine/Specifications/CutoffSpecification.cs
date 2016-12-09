using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CutoffSpecification : IDecisionEngineSpecification
    {
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public CutoffSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                _logger.Debug("Comparing file quality with report. Existing file is {0}", file.Quality);

                
                if (!_qualityUpgradableSpecification.CutoffNotMet(subject.Series.Profile, file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    _logger.Debug("Cutoff already met, rejecting.");
                    return Decision.Reject("Existing file meets cutoff: {0}", subject.Series.Profile.Value.Cutoff);
                }
            }

            return Decision.Accept();
        }
    }
}
