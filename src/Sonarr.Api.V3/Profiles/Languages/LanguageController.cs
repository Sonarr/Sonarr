using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Languages;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Profiles.Languages
{
    [V3ApiController]
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
        public List<LanguageResource> GetAll()
        {
            var languageResources = Language.All.Select(l => new LanguageResource
            {
                Id = (int)l,
                Name = l.ToString()
            })
                                    .OrderBy(l => l.Id > 0).ThenBy(l => l.Name)
                                    .ToList();

            return languageResources;
        }
    }
}
