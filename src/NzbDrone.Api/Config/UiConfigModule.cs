using System.Linq;
using System.Reflection;
using NzbDrone.Core.Configuration;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Config
{
    public class UiConfigModule : NzbDroneRestModule<UiConfigResource>
    {
        private readonly IConfigService _configService;

        public UiConfigModule(IConfigService configService)
            : base("/config/ui")
        {
            _configService = configService;

            GetResourceSingle = GetUiConfig;
            GetResourceById = GetUiConfig;
            UpdateResource = SaveUiConfig;
        }

        private UiConfigResource GetUiConfig()
        {
            var resource = new UiConfigResource();
            resource.InjectFrom(_configService);
            resource.Id = 1;

            return resource;
        }

        private UiConfigResource GetUiConfig(int id)
        {
            return GetUiConfig();
        }

        private void SaveUiConfig(UiConfigResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configService.SaveConfigDictionary(dictionary);
        }
    }
}