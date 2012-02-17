using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class RetentionSpecification
    {
        private readonly ConfigProvider _configProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public RetentionSpecification(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public RetentionSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            logger.Trace("Checking if report meets retention requirements. {0}", subject.Age);
            if (_configProvider.Retention > 0 && subject.Age > _configProvider.Retention)
            {
                logger.Trace("Quality {0} rejected by user's retention limit", subject.Age);
                return false;
            }

            return true;
        }
    }
}
