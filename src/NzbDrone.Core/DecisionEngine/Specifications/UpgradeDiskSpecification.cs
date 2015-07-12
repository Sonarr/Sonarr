using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification upgradableSpecification, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                _logger.Debug("Comparing file quality with report. Existing file is {0} - {1}", file.Quality, file.Language);

                if (!_upgradableSpecification.IsUpgradable(subject.Series.Profile, 
                                                           subject.Series.LanguageProfile, 
                                                           file.Quality, 
                                                           file.Language, 
                                                           subject.ParsedEpisodeInfo.Quality, 
                                                           subject.ParsedEpisodeInfo.Language))
                {
                    return Decision.Reject("Quality for existing file on disk is of equal or higher preference: {0} - {1}", file.Quality, file.Language);
                }
            }

            return Decision.Accept();
        }
    }
}
