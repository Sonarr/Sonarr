using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class AllowedDownloadSpecification
    {
        private readonly QualityAllowedByProfileSpecification _qualityAllowedByProfileSpecification;
        private readonly UpgradeDiskSpecification _upgradeDiskSpecification;
        private readonly AcceptableSizeSpecification _acceptableSizeSpecification;
        private readonly AlreadyInQueueSpecification _alreadyInQueueSpecification;
        private readonly RetentionSpecification _retentionSpecification;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public AllowedDownloadSpecification(QualityAllowedByProfileSpecification qualityAllowedByProfileSpecification,
            UpgradeDiskSpecification upgradeDiskSpecification, AcceptableSizeSpecification acceptableSizeSpecification,
            AlreadyInQueueSpecification alreadyInQueueSpecification, RetentionSpecification retentionSpecification)
        {
            _qualityAllowedByProfileSpecification = qualityAllowedByProfileSpecification;
            _upgradeDiskSpecification = upgradeDiskSpecification;
            _acceptableSizeSpecification = acceptableSizeSpecification;
            _alreadyInQueueSpecification = alreadyInQueueSpecification;
            _retentionSpecification = retentionSpecification;
        }

        public AllowedDownloadSpecification()
        {
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            if (!_qualityAllowedByProfileSpecification.IsSatisfiedBy(subject)) return false;
            if (!_upgradeDiskSpecification.IsSatisfiedBy(subject)) return false;
            if (!_retentionSpecification.IsSatisfiedBy(subject)) return false;
            if (!_acceptableSizeSpecification.IsSatisfiedBy(subject)) return false;
            if (_alreadyInQueueSpecification.IsSatisfiedBy(subject)) return false;

            logger.Debug("Episode {0} is needed", subject);
            return true;
        }
    }
}