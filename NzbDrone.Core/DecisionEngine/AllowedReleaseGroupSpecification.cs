using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.DecisionEngine
{
    public class AllowedReleaseGroupSpecification
    {
        private readonly ConfigProvider _configProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AllowedReleaseGroupSpecification(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public AllowedReleaseGroupSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            logger.Trace("Beginning release group check for: {0}", subject);

            var allowed = _configProvider.AllowedReleaseGroups;

            if (string.IsNullOrWhiteSpace(allowed))
                return true;

            foreach(var group in allowed.Trim(',', ' ').Split(','))
            {
                if (subject.ReleaseGroup.Equals(group.Trim(' '), StringComparison.CurrentCultureIgnoreCase))
                {
                    logger.Trace("Item: {0}'s release group is wanted: {1}", subject, subject.ReleaseGroup);
                    return true;
                }
            }
            
            logger.Trace("Item: {0}'s release group is not wanted: {1}", subject, subject.ReleaseGroup);
            return false;
        }    
    }
}
