using System;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class PackageGlobalMessageCheck : HealthCheckBase
    {
        private readonly IDeploymentInfoProvider _deploymentInfoProvider;


        public PackageGlobalMessageCheck(IDeploymentInfoProvider deploymentInfoProvider)
        {
            _deploymentInfoProvider = deploymentInfoProvider;
        }

        public override HealthCheck Check()
        {
            if (_deploymentInfoProvider.PackageGlobalMessage.IsNullOrWhiteSpace())
            {
                return new HealthCheck(GetType());
            }

            var message = _deploymentInfoProvider.PackageGlobalMessage;
            HealthCheckResult result = HealthCheckResult.Notice;
            
            if (message.StartsWith("Error:"))
            {
                message = message.Substring(6);
                result = HealthCheckResult.Error;
            }
            else if (message.StartsWith("Warn:"))
            {
                message = message.Substring(5);
                result = HealthCheckResult.Warning;
            }

            return new HealthCheck(GetType(), result, message, "#package_maintainer_message");
        }
    }
}
