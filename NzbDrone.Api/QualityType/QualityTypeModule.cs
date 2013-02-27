using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.QualityType
{
    public class QualityTypeModule : NzbDroneApiModule
    {
        private readonly QualitySizeService _qualityTypeProvider;

        public QualityTypeModule(QualitySizeService qualityTypeProvider)
            : base("/QualityTypes")
        {
            _qualityTypeProvider = qualityTypeProvider;

            Get["/"] = x => GetQualityType();
            Get["/{id}"] = x => GetQualityType(x.Id);
            Put["/"] = x => PutQualityType();
        }

        private Response PutQualityType()
        {
            var model = Request.Body.FromJson<QualityTypeModel>();

            var type = Mapper.Map<QualityTypeModel, QualitySize>(model);
            _qualityTypeProvider.Update(type);

            return model.AsResponse();

        }

        private Response GetQualityType(int id)
        {
            var type = _qualityTypeProvider.Get(id);
            return Mapper.Map<QualitySize, QualityTypeModel>(type).AsResponse();
        }

        private Response GetQualityType()
        {
            var types = _qualityTypeProvider.All().Where(qualityType => qualityType.QualityId != 0 && qualityType.QualityId != 10).ToList();
            var responseModel = Mapper.Map<List<QualitySize>, List<QualityTypeModel>>(types);

            return responseModel.AsResponse();
        }
    }
}