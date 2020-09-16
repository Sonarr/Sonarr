using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http;

namespace NzbDrone.Api.Profiles
{
    public class ProfileModule : SonarrRestModule<ProfileResource>
    {
        private readonly IQualityProfileService _qualityProfileService;

        public ProfileModule(IQualityProfileService qualityProfileService)
        {
            _qualityProfileService = qualityProfileService;
            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Cutoff).NotNull();
            SharedValidator.RuleFor(c => c.Items).MustHaveAllowedQuality();

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            CreateResource = Create;
            DeleteResource = DeleteProfile;
        }

        private int Create(ProfileResource resource)
        {
            var model = resource.ToModel();

            return _qualityProfileService.Add(model).Id;
        }

        private void DeleteProfile(int id)
        {
            _qualityProfileService.Delete(id);
        }

        private void Update(ProfileResource resource)
        {
            var model = resource.ToModel();

            _qualityProfileService.Update(model);
        }

        private ProfileResource GetById(int id)
        {
            return _qualityProfileService.Get(id).ToResource();
        }

        private List<ProfileResource> GetAll()
        {
            return _qualityProfileService.All().ToResource();
        }
    }
}
