using System.Linq;
using NLog;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class CustomStartDateSpecification
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            if (!subject.Series.CustomStartDate.HasValue)
            {
                logger.Debug("{0} does not restrict downloads before date.", subject.Series.Title);
                return true;
            }

            if (subject.Episodes.Any(episode => episode.AirDate >= subject.Series.CustomStartDate.Value))
            {
                logger.Debug("One or more episodes aired after cutoff, downloading.");
                return true;
            }

            logger.Debug("Episodes aired before cutoff date: {0}", subject.Series.CustomStartDate);
            return false;
        }
    }
}
