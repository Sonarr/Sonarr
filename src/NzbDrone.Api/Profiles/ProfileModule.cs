using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Validation;
using Sonarr.Http;
using Sonarr.Http.Mapping;

namespace NzbDrone.Api.Profiles
{
    public class ProfileModule : SonarrRestModule<ProfileResource>
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
            var model = resource.ToModel();

            return _profileService.Add(model).Id;
        }

        private void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        private void Update(ProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);
        }

        private ProfileResource GetById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        private List<ProfileResource> GetAll()
        {
            return _profileService.All().ToResource();
        }
    }
}