using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CutoffSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IPreferredWordService _preferredWordServiceCalculator;
        private readonly Logger _logger;

        public CutoffSpecification(UpgradableSpecification upgradableSpecification, IPreferredWordService preferredWordServiceCalculator, Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
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

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                                                           languageProfile, 
                                                           file.Quality, 
                                                           file.Language,
                                                           _preferredWordServiceCalculator.Calculate(subject.Series, file.GetSceneOrFileName()),
                                                           subject.ParsedEpisodeInfo.Quality,
                                                           subject.PreferredWordScore))
                {
                    _logger.Debug("Cutoff already met, rejecting.");

                    var qualityCutoffIndex = qualityProfile.GetIndex(qualityProfile.Cutoff);
                    var qualityCutoff = qualityProfile.Items[qualityCutoffIndex.Index];

                    return Decision.Reject("Existing file meets cutoff: {0} - {1}", qualityCutoff, languageProfile.Cutoff);
                }
            }

            return Decision.Accept();
        }
    }
}
