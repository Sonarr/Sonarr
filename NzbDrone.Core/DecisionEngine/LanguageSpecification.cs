using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.DecisionEngine
{
    public class LanguageSpecification
    {
        private readonly ConfigProvider _configProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public LanguageSpecification(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
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
