using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Qualities
{
    public class QualitySizeModule : NzbDroneRestModule<QualitySizeResource>
    {
        private readonly QualitySizeService _qualityTypeProvider;

        public QualitySizeModule(QualitySizeService qualityTypeProvider)
        {
            _qualityTypeProvider = qualityTypeProvider;

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;
        }

        private QualitySizeResource Update(QualitySizeResource resource)
        {
            var model = resource.InjectTo<QualitySize>();
            _qualityTypeProvider.Update(model);
            return GetById(resource.Id);
        }

        private QualitySizeResource GetById(int id)
        {
            return ToResource(() => _qualityTypeProvider.Get(id));
        }

        private List<QualitySizeResource> GetAll()
        {
            return ToListResource(_qualityTypeProvider.All);
        }
    }
}