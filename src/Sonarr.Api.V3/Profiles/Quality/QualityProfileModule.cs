using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Validation;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Quality
{
    public class ProfileModule : SonarrRestModule<QualityProfileResource>
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

        private int Create(QualityProfileResource resource)
        {
            var model = resource.ToModel();
            model = _profileService.Add(model);
            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        private void Update(QualityProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);
        }

        private QualityProfileResource GetById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        private List<QualityProfileResource> GetAll()
        {
            return _profileService.All().ToResource();
        }
    }
}