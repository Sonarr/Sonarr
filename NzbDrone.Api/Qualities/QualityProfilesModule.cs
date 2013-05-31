using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Qualities
{
    public class QualityProfilesModule : NzbDroneRestModule<QualityProfileResource>
    {
        private readonly QualityProfileService _qualityProvider;

        public QualityProfilesModule(QualityProfileService qualityProvider)
            : base("/qualityProfiles")
        {
            _qualityProvider = qualityProvider;

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;

            CreateResource = Create;

            DeleteResource = DeleteProfile;

        }

        private QualityProfileResource Create(QualityProfileResource resource)
        {
            var model = resource.InjectTo<QualityProfile>();
            model = _qualityProvider.Add(model);
            return GetById(model.Id);
        }

        private void DeleteProfile(int id)
        {
            _qualityProvider.Delete(id);
        }

        private QualityProfileResource Update(QualityProfileResource resource)
        {
            var model = resource.InjectTo<QualityProfile>();
            _qualityProvider.Update(model);
            return GetById(resource.Id);
        }

        private QualityProfileResource GetById(int id)
        {
            return ToResource(() => _qualityProvider.Get(id));
        }

        private List<QualityProfileResource> GetAll()
        {
            return ToListResource(_qualityProvider.All);
        }

    }
}