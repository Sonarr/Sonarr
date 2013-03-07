using NLog;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class AllowedDownloadSpecification
    {
        private readonly QualityAllowedByProfileSpecification _qualityAllowedByProfileSpecification;
        private readonly UpgradeDiskSpecification _upgradeDiskSpecification;
        private readonly AcceptableSizeSpecification _acceptableSizeSpecification;
        private readonly NotInQueueSpecification _notInQueueSpecification;
        private readonly RetentionSpecification _retentionSpecification;
        private readonly AllowedReleaseGroupSpecification _allowedReleaseGroupSpecification;
        private readonly CustomStartDateSpecification _customStartDateSpecification;
        private readonly LanguageSpecification _languageSpecification;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AllowedDownloadSpecification(QualityAllowedByProfileSpecification qualityAllowedByProfileSpecification,
            UpgradeDiskSpecification upgradeDiskSpecification, AcceptableSizeSpecification acceptableSizeSpecification,
            NotInQueueSpecification notInQueueSpecification, RetentionSpecification retentionSpecification,
            AllowedReleaseGroupSpecification allowedReleaseGroupSpecification, CustomStartDateSpecification customStartDateSpecification,
            LanguageSpecification languageSpecification)
        {
            _qualityAllowedByProfileSpecification = qualityAllowedByProfileSpecification;
            _upgradeDiskSpecification = upgradeDiskSpecification;
            _acceptableSizeSpecification = acceptableSizeSpecification;
            _notInQueueSpecification = notInQueueSpecification;
            _retentionSpecification = retentionSpecification;
            _allowedReleaseGroupSpecification = allowedReleaseGroupSpecification;
            _customStartDateSpecification = customStartDateSpecification;
            _languageSpecification = languageSpecification;
        }

        public AllowedDownloadSpecification()
        {
        }

        public virtual ReportRejectionReasons IsSatisfiedBy(EpisodeParseResult subject)
        {
            if (!_qualityAllowedByProfileSpecification.IsSatisfiedBy(subject)) return ReportRejectionReasons.QualityNotWanted;
            if (!_customStartDateSpecification.IsSatisfiedBy(subject)) return ReportRejectionReasons.AiredAfterCustomStartDate;
            if (!_upgradeDiskSpecification.IsSatisfiedBy(subject)) return ReportRejectionReasons.ExistingQualityIsEqualOrBetter;
            if (!_languageSpecification.IsSatisfiedBy(subject)) return ReportRejectionReasons.LanguageNotWanted;
            if (!_retentionSpecification.IsSatisfiedBy(subject)) return ReportRejectionReasons.Retention;
            if (!_acceptableSizeSpecification.IsSatisfiedBy(subject)) return ReportRejectionReasons.Size;
            if (!_allowedReleaseGroupSpecification.IsSatisfiedBy(subject)) return ReportRejectionReasons.ReleaseGroupNotWanted;
            if (!_notInQueueSpecification.IsSatisfiedBy(subject)) return ReportRejectionReasons.AlreadyInQueue;
            
            logger.Debug("Episode {0} is needed", subject);
            return ReportRejectionReasons.None;
        }
    }
}