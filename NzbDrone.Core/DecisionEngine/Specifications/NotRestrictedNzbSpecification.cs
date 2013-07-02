using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotRestrictedNzbSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public NotRestrictedNzbSpecification(IConfigService configService, Logger logger)
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
            _logger.Trace("Checking if Nzb contains any restrictions: {0}", subject);

            var restrictionsString = _configService.NzbRestrictions;

            if (String.IsNullOrWhiteSpace(restrictionsString))
            {
                _logger.Trace("No restrictions configured, allowing: {0}", subject);
                return true;
            }

            var restrictions = restrictionsString.Split('\n');

            foreach (var restriction in restrictions)
            {
                if (subject.Report.Title.Contains(restriction))
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
