using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class RetentionSpecification
    {
        private readonly IConfigService _configService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public RetentionSpecification(IConfigService configService)
        {
            _configService = configService;
        }

        public RetentionSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            logger.Trace("Checking if report meets retention requirements. {0}", subject.Age);
            if (_configService.Retention > 0 && subject.Age > _configService.Retention)
            {
                logger.Trace("Report age: {0} rejected by user's retention limit", subject.Age);
                return false;
            }

            return true;
        }
    }
}
