using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Localization;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Localization;

[V5ApiController]
public class LocalizationController : RestController<LocalizationResource>
{
    private readonly ILocalizationService _localizationService;

    public LocalizationController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    protected override LocalizationResource GetResourceById(int id)
    {
        return _localizationService.GetLocalizationDictionary().ToResource();
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<LocalizationResource> GetLocalization()
    {
        return TypedResults.Ok(GetResourceById(1));
    }

    [HttpGet("language")]
    [Produces("application/json")]
    public Ok<LocalizationLanguageResource> GetLanguage()
    {
        var identifier = _localizationService.GetLanguageIdentifier();

        return TypedResults.Ok(new LocalizationLanguageResource
        {
            Identifier = identifier
        });
    }
}
