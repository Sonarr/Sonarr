using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Mapping;
using System.Linq;

namespace NzbDrone.Api.Qualities
{

    public static class LazyLoadedExtensions
    {
        public static IEnumerable<int> GetForeignKeys(this IEnumerable<ModelBase> models)
        {
            return models.Select(c => c.Id).Distinct();
        }
    }

    public class QualityProfileModule : NzbDroneRestModule<QualityProfileResource>
    {
        private readonly QualityProfileService _qualityProfileService;

        public QualityProfileModule(QualityProfileService qualityProfileService)
            : base("/qualityprofiles")
        {
            _qualityProfileService = qualityProfileService;

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;

            CreateResource = Create;

            DeleteResource = DeleteProfile;
        }

        private QualityProfileResource Create(QualityProfileResource resource)
        {
            var model = resource.InjectTo<QualityProfile>();
            model = _qualityProfileService.Add(model);
            return GetById(model.Id);
        }

        private void DeleteProfile(int id)
        {
            _qualityProfileService.Delete(id);
        }

        private QualityProfileResource Update(QualityProfileResource resource)
        {
            var model = resource.InjectTo<QualityProfile>();
            _qualityProfileService.Update(model);
            return GetById(resource.Id);
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