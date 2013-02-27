using System.Linq;
using NLog;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class QualityUpgradeSpecification
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual bool IsSatisfiedBy(QualityModel currentQuality, QualityModel newQuality, Quality cutOff)
        {
            if (currentQuality >= newQuality)
            {
                logger.Trace("existing item has better or equal quality. skipping");
                return false;
            }

            if (currentQuality.Quality == newQuality.Quality && newQuality.Proper)
            {
                logger.Trace("Upgrading existing item to proper.");
                return true;
            }

            if (currentQuality.Quality >= cutOff)
            {
                logger.Trace("Existing item meets cut-off. skipping.");
                return false;
            }

            return true;
        }
    }
}
