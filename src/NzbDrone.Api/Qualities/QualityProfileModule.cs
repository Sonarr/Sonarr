using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Mapping;
using System.Linq;
using FluentValidation;

namespace NzbDrone.Api.Qualities
{
    public class QualityProfileModule : NzbDroneRestModule<QualityProfileResource>
    {
        private readonly IQualityProfileService _qualityProfileService;
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityProfileModule(IQualityProfileService qualityProfileService,
                                    IQualityDefinitionService qualityDefinitionService)
            : base("/qualityprofiles")
        {
            _qualityProfileService = qualityProfileService;
            _qualityDefinitionService = qualityDefinitionService;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Cutoff).NotNull();
            SharedValidator.RuleFor(c => c.Items).NotEmpty();

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;

            CreateResource = Create;

            DeleteResource = DeleteProfile;
        }

        private int Create(QualityProfileResource resource)
        {
            var model = resource.InjectTo<QualityProfile>();
            model = _qualityProfileService.Add(model);
            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            _qualityProfileService.Delete(id);
        }

        private void Update(QualityProfileResource resource)
        {
            var model = _qualityProfileService.Get(resource.Id);
            
            model.Name = resource.Name;
            model.Cutoff = (Quality)resource.Cutoff.Id;
            model.Items = resource.Items.InjectTo<List<QualityProfileItem>>();
            _qualityProfileService.Update(model);
        }

        private QualityProfileResource GetById(int id)
        {
            return _qualityProfileService.Get(id).InjectTo<QualityProfileResource>();
        }

        private List<QualityProfileResource> GetAll()
        {
            var profiles = _qualityProfileService.All().InjectTo<List<QualityProfileResource>>();

            return profiles;
        }
    }
}