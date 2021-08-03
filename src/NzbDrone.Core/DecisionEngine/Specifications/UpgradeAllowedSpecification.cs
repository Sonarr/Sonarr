using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeAllowedSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;

        public UpgradeAllowedSpecification(UpgradableSpecification upgradableSpecification, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var qualityProfile = subject.Series.QualityProfile.Value;
            var languageProfile = subject.Series.LanguageProfile.Value;

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                if (file == null)
                {
                    _logger.Debug("File is no longer available, skipping this file.");
                    continue;
                }

                _logger.Debug("Comparing file quality and language with report. Existing file is {0} - {1}", file.Quality, file.Language);

                if (!_upgradableSpecification.IsUpgradeAllowed(qualityProfile,
                                                               languageProfile,
                                                               file.Quality,
                                                               file.Language,
                                                               subject.ParsedEpisodeInfo.Quality,
                                                               subject.ParsedEpisodeInfo.Language))
                {
                    _logger.Debug("Upgrading is not allowed by the quality or language profile");

                    return Decision.Reject("Existing file and the Quality or Language profile does not allow upgrades");
                }
            }

            return Decision.Accept();
        }
    }
}
