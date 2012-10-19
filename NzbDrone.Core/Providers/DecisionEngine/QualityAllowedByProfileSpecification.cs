using System.Linq;
using NLog;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class QualityAllowedByProfileSpecification
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            logger.Trace("Checking if report meets quality requirements. {0}", subject.Quality);
            if (!subject.Series.QualityProfile.Allowed.Contains(subject.Quality.Quality))
            {
                logger.Trace("Quality {0} rejected by Series' quality profile", subject.Quality);
                return false;
            }

            return true;
        }
    }
}
