using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Profiles.Languages;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Profiles.Language
{
    [V3ApiController]
    public class LanguageProfileController : RestController<LanguageProfileResource>
    {
        private readonly ILanguageProfileService _profileService;

        public LanguageProfileController(ILanguageProfileService profileService)
        {
            _profileService = profileService;
            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Cutoff).NotNull();
            SharedValidator.RuleFor(c => c.Languages).MustHaveAllowedLanguage();
        }

        [RestPostById]
        public ActionResult<LanguageProfileResource> Create(LanguageProfileResource resource)
        {
            var model = resource.ToModel();
            model = _profileService.Add(model);
            return Created(model.Id);
        }

        [RestDeleteById]
        public void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        [RestPutById]
        public ActionResult<LanguageProfileResource> Update(LanguageProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);

            return Accepted(model.Id);
        }

        protected override LanguageProfileResource GetResourceById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        [HttpGet]
        public List<LanguageProfileResource> GetAll()
        {
            return _profileService.All().ToResource();
        }
    }
}
