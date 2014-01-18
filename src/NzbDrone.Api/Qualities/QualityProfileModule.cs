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
            SharedValidator.RuleFor(c => c.Allowed).NotEmpty();

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
            model.Allowed = resource.Allowed.Select(p => (Quality)p.Id).ToList();
            _qualityProfileService.Update(model);
        }

        private QualityProfileResource GetById(int id)
        {
            return MapToResource(_qualityProfileService.Get(id));
        }

        private List<QualityProfileResource> GetAll()
        {
            var profiles = _qualityProfileService.All().Select(MapToResource).ToList();

            return profiles;
        }

        private QualityProfileResource MapToResource(QualityProfile profile)
        {
            return new QualityProfileResource
                {
                    Cutoff = MapToResource(_qualityDefinitionService.Get(profile.Cutoff)),
                    Available = _qualityDefinitionService.All()
                        .Where(c => !profile.Allowed.Any(q => c.Quality == q))
                        .Select(MapToResource).ToList(),
                    Allowed = profile.Allowed.Select(_qualityDefinitionService.Get).Select(MapToResource).ToList(),
                    Name = profile.Name,
                    Id = profile.Id
                };
        }

        private QualityResource MapToResource(QualityDefinition config)
        {
            return new QualityResource
            {
                Id = config.Quality.Id,
                Name = config.Quality.Name,
                Weight = config.Weight
            };
        }
    }
}