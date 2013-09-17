using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Qualities
{
    public class QualitySizeModule : NzbDroneRestModule<QualitySizeResource>
    {
        private readonly IQualitySizeService _qualityTypeProvider;

        public QualitySizeModule(IQualitySizeService qualityTypeProvider)
        {
            _qualityTypeProvider = qualityTypeProvider;

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;
        }

        private void Update(QualitySizeResource resource)
        {
            var model = resource.InjectTo<QualitySize>();
            _qualityTypeProvider.Update(model);
        }

        private QualitySizeResource GetById(int id)
        {
            return _qualityTypeProvider.Get(id).InjectTo<QualitySizeResource>();
        }

        private List<QualitySizeResource> GetAll()
        {
            return ToListResource(_qualityTypeProvider.All);
        }
    }
}