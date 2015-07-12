using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Api.LanguageProfiles
{
    public class LanguageProfileModule : NzbDroneRestModule<LanguageProfileResource>
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
            var model = resource.InjectTo<LanguageProfile>();
            model = _profileService.Add(model);
            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        private void Update(LanguageProfileResource resource)
        {
            var model = _profileService.Get(resource.Id);
            
            model.Name = resource.Name;
            model.Cutoff = (Language)resource.Cutoff.Id;
            model.Languages = resource.Languages.InjectTo<List<ProfileLanguageItem>>();

            _profileService.Update(model);
        }

        private LanguageProfileResource GetById(int id)
        {
            return _profileService.Get(id).InjectTo<LanguageProfileResource>();
        }

        private List<LanguageProfileResource> GetAll()
        {
            var profiles = _profileService.All().InjectTo<List<LanguageProfileResource>>();

            return profiles;
        }
    }
}