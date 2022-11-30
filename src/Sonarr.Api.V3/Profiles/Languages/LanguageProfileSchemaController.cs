using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Languages;
using NzbDrone.Http.REST.Attributes;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Profiles.Languages
{
    [V3ApiController("languageprofile/schema")]
    public class LanguageProfileSchemaController : RestController<LanguageProfileResource>
    {
        [HttpGet]
        [Deprecated]
        public LanguageProfileResource GetSchema()
        {
            return new LanguageProfileResource
            {
                Id = 1,
                Name = "Deprecated",
                UpgradeAllowed = true,
                Cutoff = Language.English,
                Languages = new List<LanguageProfileItemResource>
                {
                    new LanguageProfileItemResource
                    {
                        Language = Language.English,
                        Allowed = true
                    }
                }
            };
        }

        protected override LanguageProfileResource GetResourceById(int id)
        {
            throw new global::System.NotImplementedException();
        }
    }
}
