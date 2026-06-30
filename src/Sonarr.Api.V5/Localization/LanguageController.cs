using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Languages;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Localization;

[V5ApiController]
public class LanguageController : RestController<LanguageResource>
{
    protected override LanguageResource GetResourceById(int id)
    {
        var language = (Language)id;

        return new LanguageResource
        {
            Id = (int)language,
            Name = language.ToString()
        };
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<List<LanguageResource>> GetAll()
    {
        var languageResources = Language.All.Select(l => new LanguageResource
        {
            Id = (int)l,
            Name = l.ToString()
        })
                                .OrderBy(l => l.Id > 0).ThenBy(l => l.Name)
                                .ToList();

        return TypedResults.Ok(languageResources);
    }
}
