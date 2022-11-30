using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Languages;
using NzbDrone.Http.REST.Attributes;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Profiles.Languages
{
    [V3ApiController]
    public class LanguageProfileController : RestController<LanguageProfileResource>
    {
        [RestPostById]
        [Deprecated]
        public ActionResult<LanguageProfileResource> Create(LanguageProfileResource resource)
        {
            return Accepted(resource);
        }

        [RestDeleteById]
        [Deprecated]
        public void DeleteProfile(int id)
        {
        }

        [RestPutById]
        [Deprecated]
        public ActionResult<LanguageProfileResource> Update(LanguageProfileResource resource)
        {
            return Accepted(resource);
        }

        [RestGetById]
        [Deprecated]
        [Produces("application/json")]
        protected override LanguageProfileResource GetResourceById(int id)
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

        [HttpGet]
        [Deprecated]
        [Produces("application/json")]
        public ActionResult<List<LanguageProfileResource>> GetAll()
        {
            return new List<LanguageProfileResource>
            {
                new LanguageProfileResource
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
                }
            };
        }
    }
}
