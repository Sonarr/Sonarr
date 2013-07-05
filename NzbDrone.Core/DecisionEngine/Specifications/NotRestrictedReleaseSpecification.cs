using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotRestrictedReleaseSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public NotRestrictedReleaseSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Contrains restricted term.";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject)
        {
            _logger.Trace("Checking if release contains any restricted terms: {0}", subject);

            var restrictionsString = _configService.ReleaseRestrictions;

            if (String.IsNullOrWhiteSpace(restrictionsString))
            {
                _logger.Trace("No restrictions configured, allowing: {0}", subject);
                return true;
            }

            var restrictions = restrictionsString.Split('\n');

            foreach (var restriction in restrictions)
            {
                if (subject.Report.Title.ToLowerInvariant().Contains(restriction.ToLowerInvariant()))
                {
                    _logger.Trace("{0} is restricted: {1}", subject, restriction);
                    return false;
                }
            }

            _logger.Trace("No restrictions apply, allowing: {0}", subject);
            return true;
        }
    }
}
