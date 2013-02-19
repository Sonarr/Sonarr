using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Search;

namespace NzbDrone.Core.DecisionEngine
{
    public class AllowedDownloadSpecification
    {
        private readonly QualityAllowedByProfileSpecification _qualityAllowedByProfileSpecification;
        private readonly UpgradeDiskSpecification _upgradeDiskSpecification;
        private readonly AcceptableSizeSpecification _acceptableSizeSpecification;
        private readonly AlreadyInQueueSpecification _alreadyInQueueSpecification;
        private readonly RetentionSpecification _retentionSpecification;
        private readonly AllowedReleaseGroupSpecification _allowedReleaseGroupSpecification;
        private readonly CustomStartDateSpecification _customStartDateSpecification;
        private readonly LanguageSpecification _languageSpecification;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AllowedDownloadSpecification(QualityAllowedByProfileSpecification qualityAllowedByProfileSpecification,
            UpgradeDiskSpecification upgradeDiskSpecification, AcceptableSizeSpecification acceptableSizeSpecification,
            AlreadyInQueueSpecification alreadyInQueueSpecification, RetentionSpecification retentionSpecification,
            AllowedReleaseGroupSpecification allowedReleaseGroupSpecification, CustomStartDateSpecification customStartDateSpecification,
            LanguageSpecification languageSpecification)
        {
            _qualityAllowedByProfileSpecification = qualityAllowedByProfileSpecification;
            _upgradeDiskSpecification = upgradeDiskSpecification;
            _acceptableSizeSpecification = acceptableSizeSpecification;
            _alreadyInQueueSpecification = alreadyInQueueSpecification;
            _retentionSpecification = retentionSpecification;
            _allowedReleaseGroupSpecification = allowedReleaseGroupSpecification;
            _customStartDateSpecification = customStartDateSpecification;
            _languageSpecification = languageSpecification;
        }

        public AllowedDownloadSpecification()
        {
        }

        public virtual ReportRejectionType IsSatisfiedBy(EpisodeParseResult subject)
        {
            if (!_qualityAllowedByProfileSpecification.IsSatisfiedBy(subject)) return ReportRejectionType.QualityNotWanted;
            if (!_customStartDateSpecification.IsSatisfiedBy(subject)) return ReportRejectionType.AiredAfterCustomStartDate;
            if (!_upgradeDiskSpecification.IsSatisfiedBy(subject)) return ReportRejectionType.ExistingQualityIsEqualOrBetter;
            if (!_languageSpecification.IsSatisfiedBy(subject)) return ReportRejectionType.LanguageNotWanted;
            if (!_retentionSpecification.IsSatisfiedBy(subject)) return ReportRejectionType.Retention;
            if (!_acceptableSizeSpecification.IsSatisfiedBy(subject)) return ReportRejectionType.Size;
            if (!_allowedReleaseGroupSpecification.IsSatisfiedBy(subject)) return ReportRejectionType.ReleaseGroupNotWanted;
            if (_alreadyInQueueSpecification.IsSatisfiedBy(subject)) return ReportRejectionType.AlreadyInQueue;
            
            logger.Debug("Episode {0} is needed", subject);
            return ReportRejectionType.None;
        }
    }
}