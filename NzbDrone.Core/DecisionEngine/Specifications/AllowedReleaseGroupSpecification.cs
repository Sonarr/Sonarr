using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AllowedReleaseGroupSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AllowedReleaseGroupSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }


        public string RejectionReason
        {
            get
            {
                return "Release group is blacklisted.";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject)
        {
            _logger.Trace("Beginning release group check for: {0}", subject);

            //Todo: Make this use NzbRestrictions - How should whitelist be used? Will it override blacklist or vice-versa?

            var allowed = _configService.AllowedReleaseGroups;

            if (string.IsNullOrWhiteSpace(allowed))
                return true;

            var reportReleaseGroup = subject.Report.ReleaseGroup.ToLower();

            return allowed.ToLower().Split(',').Any(allowedGroup => allowedGroup.Trim() == reportReleaseGroup);
        }
    }
}
