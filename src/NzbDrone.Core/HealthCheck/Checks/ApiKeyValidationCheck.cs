using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ApplicationStartedEvent))]
    [CheckOn(typeof(ConfigSavedEvent))]
    public class ApiKeyValidationCheck : HealthCheckBase
    {
        private const int MinimumLength = 20;

        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public ApiKeyValidationCheck(IConfigFileProvider configFileProvider, Logger logger, ILocalizationService localizationService)
            : base(localizationService)
        {
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (_configFileProvider.ApiKey.Length < MinimumLength)
            {
                _logger.Warn("Please update your API key to be at least {0} characters long. You can do this via settings or the config file", MinimumLength);

                return new HealthCheck(GetType(), HealthCheckResult.Warning, _localizationService.GetLocalizedString("ApiKeyValidationHealthCheckMessage", new Dictionary<string, object> { { "length", MinimumLength } }), "#invalid-api-key");
            }

            return new HealthCheck(GetType());
        }
    }
}
