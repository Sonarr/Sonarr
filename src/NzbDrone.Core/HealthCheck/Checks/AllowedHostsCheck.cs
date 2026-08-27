using NLog;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ApplicationStartedEvent))]
    [CheckOn(typeof(ConfigFileSavedEvent))]
    public class AllowedHostsCheck : HealthCheckBase
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public AllowedHostsCheck(IConfigFileProvider configFileProvider, ILocalizationService localizationService, Logger logger)
            : base(localizationService)
        {
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            if (AllowedHostsParser.Parse(_configFileProvider.AllowedHosts).Count > 0)
            {
                return new HealthCheck(GetType());
            }

            if (_configFileProvider.AuthenticationRequired != AuthenticationRequiredType.Enabled)
            {
                _logger.Warn("Allowed Hosts is not configured, requests for any hostname will be accepted. You can set this via settings or the config file");

                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    HealthCheckReason.AllowedHostsNotConfigured,
                    _localizationService.GetLocalizedString("AllowedHostsNotConfiguredHealthCheckMessage"),
                    "#allowed-hosts-not-configured");
            }

            return new HealthCheck(GetType());
        }
    }
}
