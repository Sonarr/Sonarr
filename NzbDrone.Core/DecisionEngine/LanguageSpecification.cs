using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class LanguageSpecification
    {
        private readonly IConfigService _configService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public LanguageSpecification(IConfigService configService)
        {
            _configService = configService;
        }

        public LanguageSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            logger.Trace("Checking if report meets language requirements. {0}", subject.Language);
            if (subject.Language != LanguageType.English)
            {
                logger.Trace("Report Language: {0} rejected because it is not english", subject.Language);
                return false;
            }

            return true;
        }
    }
}
