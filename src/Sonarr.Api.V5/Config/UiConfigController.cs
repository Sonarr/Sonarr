using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Config;

[V5ApiController("config/ui")]
public class UiConfigController : RestController<UiConfigResource>
{
    private readonly IConfigFileProvider _configFileProvider;
    private readonly IConfigService _configService;

    public UiConfigController(IConfigFileProvider configFileProvider, IConfigService configService)
    {
        _configFileProvider = configFileProvider;
        _configService = configService;
    }

    [HttpGet]
    public UiConfigResource GetUiConfig()
    {
        return UiConfigResourceMapper.ToResource(_configFileProvider, _configService);
    }

    [HttpPut]
    public ActionResult<UiConfigResource> SaveUiConfig([FromBody] UiConfigResource resource)
    {
        var dictionary = resource.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

        _configFileProvider.SaveConfigDictionary(dictionary);
        _configService.SaveConfigDictionary(dictionary);

        return Accepted(resource);
    }
}
