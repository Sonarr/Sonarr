using System.Linq;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(UpgradableSpecification upgradableSpecification,
                                        ICustomFormatCalculationService formatService,
                                        Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var qualityProfile = subject.Series.QualityProfile.Value;

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                if (file == null)
                {
                    _logger.Debug("File is no longer available, skipping this file.");
                    continue;
                }

                _logger.Debug("Comparing file quality with report. Existing file is {0}.", file.Quality);

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                        file.Quality,
                        _formatService.ParseCustomFormat(file),
                        subject.ParsedEpisodeInfo.Quality))
                {
                    _logger.Debug("Cutoff already met, rejecting.");

                    var cutoff = qualityProfile.UpgradeAllowed ? qualityProfile.Cutoff : qualityProfile.FirststAllowedQuality().Id;
                    var qualityCutoff = qualityProfile.Items[qualityProfile.GetIndex(cutoff).Index];

                    return Decision.Reject("Existing file meets cutoff: {0}", qualityCutoff);
                }

                var customFormats = _formatService.ParseCustomFormat(file);

                var upgradeableRejectReason = _upgradableSpecification.IsUpgradable(qualityProfile,
                    file.Quality,
                    customFormats,
                    subject.ParsedEpisodeInfo.Quality,
                    subject.CustomFormats);

                switch (upgradeableRejectReason)
                {
                    case UpgradeableRejectReason.None:
                        continue;

                    case UpgradeableRejectReason.BetterQuality:
                        return Decision.Reject("Existing file on disk is of equal or higher preference: {0}", file.Quality);

                    case UpgradeableRejectReason.BetterRevision:
                        return Decision.Reject("Existing file on disk is of equal or higher revision: {0}", file.Quality.Revision);

                    case UpgradeableRejectReason.QualityCutoff:
                        return Decision.Reject("Existing file on disk meets quality cutoff: {0}", qualityProfile.Items[qualityProfile.GetIndex(qualityProfile.Cutoff).Index]);

                    case UpgradeableRejectReason.CustomFormatCutoff:
                        return Decision.Reject("Existing file on disk meets Custom Format cutoff: {0}", qualityProfile.CutoffFormatScore);

                    case UpgradeableRejectReason.CustomFormatScore:
                        return Decision.Reject("Existing file on disk has a equal or higher Custom Format score: {0}", qualityProfile.CalculateCustomFormatScore(customFormats));

                    case UpgradeableRejectReason.MinCustomFormatScore:
                        return Decision.Reject("Existing file on disk has Custom Format score within Custom Format score increment: {0}", qualityProfile.MinUpgradeFormatScore);
                }
            }

            return Decision.Accept();
        }
    }
}
