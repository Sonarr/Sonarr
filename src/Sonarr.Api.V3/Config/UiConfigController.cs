using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using Sonarr.Http;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Config
{
    [V3ApiController("config/ui")]
    public class UiConfigController : ConfigController<UiConfigResource>
    {
        private readonly IConfigFileProvider _configFileProvider;

        public UiConfigController(IConfigFileProvider configFileProvider, IConfigService configService)
            : base(configService)
        {
            _configFileProvider = configFileProvider;
        }

        [RestPutById]
        public override ActionResult<UiConfigResource> SaveConfig(UiConfigResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configFileProvider.SaveConfigDictionary(dictionary);
            _configService.SaveConfigDictionary(dictionary);

            return Accepted(resource.Id);
        }

        protected override UiConfigResource ToResource(IConfigService model)
        {
            return UiConfigResourceMapper.ToResource(_configFileProvider, model);
        }
    }
}
