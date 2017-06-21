using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigSavedEvent))]
    public class DeprecatedDroneFactoryCheck : HealthCheckBase
    {
        private readonly IConfigService _configService;

        public DeprecatedDroneFactoryCheck(IConfigService configService)
        {
            _configService = configService;
        }

        public override HealthCheck Check()
        {
            var droneFactoryFolder = _configService.DownloadedEpisodesFolder;

            if (droneFactoryFolder.IsNullOrWhiteSpace())
            {
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType(), HealthCheckResult.Warning, "Drone Factory is deprecated and should not be used", "#drone-factory-is-deprecated");
        }
    }
}
