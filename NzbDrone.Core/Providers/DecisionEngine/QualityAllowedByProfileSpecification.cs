using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class QualityUpgradeSpecification
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual bool IsSatisfiedBy(Quality currentQuality, Quality newQuality, QualityTypes cutOff)
        {
            if (currentQuality >= newQuality)
            {
                logger.Trace("existing item has better or equal quality. skipping");
                return false;
            }

            if (currentQuality.QualityType == newQuality.QualityType && newQuality.Proper)
            {
                logger.Trace("Upgrading existing item to proper.");
                return true;
            }

            if (currentQuality.QualityType >= cutOff)
            {
                logger.Trace("Existing item meets cut-off. skipping.");
                return false;
            }

            return true;
        }
    }
}
