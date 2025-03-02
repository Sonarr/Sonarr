using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update;
using NzbDrone.Core.Validation.Paths;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Settings;

[V5ApiController("settings/update")]
public class UpdateSettingsController : RestController<UpdateSettingsResource>
{
    private readonly IConfigFileProvider _configFileProvider;

    public UpdateSettingsController(IConfigFileProvider configFileProvider)
    {
        _configFileProvider = configFileProvider;
        SharedValidator.RuleFor(c => c.UpdateScriptPath)
            .IsValidPath()
            .When(c => c.UpdateMechanism == UpdateMechanism.Script);
    }

    [HttpGet]
    public UpdateSettingsResource GetUpdateSettings()
    {
        var resource = new UpdateSettingsResource
        {
            Branch = _configFileProvider.Branch,
            UpdateAutomatically = _configFileProvider.UpdateAutomatically,
            UpdateMechanism = _configFileProvider.UpdateMechanism,
            UpdateScriptPath = _configFileProvider.UpdateScriptPath
        };

        return resource;
    }

    [HttpPut]
    public ActionResult<UpdateSettingsResource> SaveUpdateSettings([FromBody] UpdateSettingsResource resource)
    {
        var dictionary = resource.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

        _configFileProvider.SaveConfigDictionary(dictionary);

        return Accepted(resource);
    }
}
