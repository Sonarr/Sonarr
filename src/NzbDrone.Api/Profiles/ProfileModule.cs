using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Validation;

namespace NzbDrone.Api.Profiles
{
    public class ProfileModule : NzbDroneRestModule<ProfileResource>
    {
        private readonly IProfileService _profileService;

        public ProfileModule(IProfileService profileService)
        {
            _profileService = profileService;
            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Cutoff).NotNull();
            SharedValidator.RuleFor(c => c.Items).MustHaveAllowedQuality();
            SharedValidator.RuleFor(c => c.Language).ValidLanguage();

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            CreateResource = Create;
            DeleteResource = DeleteProfile;
        }

        private int Create(ProfileResource resource)
        {
            var model = resource.InjectTo<Profile>();
            model = _profileService.Add(model);
            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        private void Update(ProfileResource resource)
        {
            var model = _profileService.Get(resource.Id);
            
            model.Name = resource.Name;
            model.Cutoff = (Quality)resource.Cutoff.Id;
            model.Items = resource.Items.InjectTo<List<ProfileQualityItem>>();
            model.Language = resource.Language;

            _profileService.Update(model);
        }

        private ProfileResource GetById(int id)
        {
            return _profileService.Get(id).InjectTo<ProfileResource>();
        }

        private List<ProfileResource> GetAll()
        {
            var profiles = _profileService.All().InjectTo<List<ProfileResource>>();

            return profiles;
        }
    }
}