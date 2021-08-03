using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Quality
{
    public class ProfileModule : SonarrRestModule<QualityProfileResource>
    {
        private readonly IQualityProfileService _qualityProfileService;

        public ProfileModule(IQualityProfileService qualityProfileService)
        {
            _qualityProfileService = qualityProfileService;
            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Cutoff).ValidCutoff();
            SharedValidator.RuleFor(c => c.Items).ValidItems();

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            CreateResource = Create;
            DeleteResource = DeleteProfile;
        }

        private int Create(QualityProfileResource resource)
        {
            var model = resource.ToModel();
            model = _qualityProfileService.Add(model);
            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            _qualityProfileService.Delete(id);
        }

        private void Update(QualityProfileResource resource)
        {
            var model = resource.ToModel();

            _qualityProfileService.Update(model);
        }

        private QualityProfileResource GetById(int id)
        {
            return _qualityProfileService.Get(id).ToResource();
        }

        private List<QualityProfileResource> GetAll()
        {
            return _qualityProfileService.All().ToResource();
        }
    }
}
