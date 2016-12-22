using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CutoffSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;

        public CutoffSpecification(UpgradableSpecification UpgradableSpecification, Logger logger)
        {
            _upgradableSpecification = UpgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                _logger.Debug("Comparing file quality with report. Existing file is {0} - {1}", file.Quality, file.Language);


                if (!_upgradableSpecification.CutoffNotMet(subject.Series.Profile, 
                                                           subject.Series.LanguageProfile, 
                                                           file.Quality, 
                                                           file.Language, 
                                                           subject.ParsedEpisodeInfo.Quality))
                {
                    _logger.Debug("Cutoff already met, rejecting.");
                    return Decision.Reject("Existing file meets cutoff: {0} - {1}", subject.Series.Profile.Value.Cutoff, subject.Series.LanguageProfile.Value.Cutoff);
                }
            }

            return Decision.Accept();
        }
    }
}
