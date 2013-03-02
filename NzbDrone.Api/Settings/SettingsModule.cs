using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Settings
{
    public class SettingsModule : NzbDroneApiModule
    {
        private readonly ConfigService _configService;

        public SettingsModule(ConfigService configService)
            : base("/settings")
        {
            _configService = configService;
            Get["/"] = x => GetAllSettings();
        }

        private Response GetAllSettings()
        {
            var settings = _configService.All();
            return settings.AsResponse();
        }
    }
}