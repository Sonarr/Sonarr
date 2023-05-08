using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ApplicationStartedEvent))]
    [CheckOn(typeof(ConfigSavedEvent))]
    public class ApiKeyValidationCheck : HealthCheckBase
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public ApiKeyValidationCheck(IConfigFileProvider configFileProvider, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (_configFileProvider.ApiKey.Length < 20)
            {
                _logger.Warn("Please update your API key to be at least 20 characters long. You can do this via settings or the config file");

                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Please update your API key to be at least 20 characters long. You can do this via settings or the config file", "#invalid-api-key");
            }

            return new HealthCheck(GetType());
        }
    }
}
