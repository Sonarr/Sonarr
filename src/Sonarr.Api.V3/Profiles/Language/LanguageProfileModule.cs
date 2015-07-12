using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Profiles.Languages;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Language
{
    public class LanguageProfileModule : SonarrRestModule<LanguageProfileResource>
    {
        private readonly ILanguageProfileService _profileService;

        public LanguageProfileModule(ILanguageProfileService profileService)
        {
            _profileService = profileService;
            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Cutoff).NotNull();
            SharedValidator.RuleFor(c => c.Languages).MustHaveAllowedLanguage();

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            CreateResource = Create;
            DeleteResource = DeleteProfile;
        }

        private int Create(LanguageProfileResource resource)
        {
            var model = resource.ToModel();
            model = _profileService.Add(model);
            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        private void Update(LanguageProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);
        }

        private LanguageProfileResource GetById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        private List<LanguageProfileResource> GetAll()
        {
            var profiles = _profileService.All().ToResource();

            return profiles;
        }
    }
}