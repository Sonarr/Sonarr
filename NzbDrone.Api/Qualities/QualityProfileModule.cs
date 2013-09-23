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

        public QualityProfileModule(IQualityProfileService qualityProfileService)
            : base("/qualityprofiles")
        {
            _qualityProfileService = qualityProfileService;

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
            var model = resource.InjectTo<QualityProfile>();
            _qualityProfileService.Update(model);
        }

        private QualityProfileResource GetById(int id)
        {
            return QualityToResource(_qualityProfileService.Get(id));
        }

        private List<QualityProfileResource> GetAll()
        {
            var allProfiles = _qualityProfileService.All();


            var profiles = allProfiles.Select(QualityToResource).ToList();

            return profiles;
        }

        private static QualityProfileResource QualityToResource(QualityProfile profile)
        {
            return new QualityProfileResource
                {
                    Cutoff = profile.Cutoff.InjectTo<QualityResource>(),
                    Available = Quality.All()
                        .Where(c => !profile.Allowed.Any(q => c.Id == q.Id))
                        .InjectTo<List<QualityResource>>(),

                    Allowed = profile.Allowed.InjectTo<List<QualityResource>>(),
                    Name = profile.Name,
                    Id = profile.Id
                };
        }
    }
}