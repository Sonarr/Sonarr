using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Providers;

namespace NzbDrone.Api.QualityType
{
    public class QualityTypeModule : NzbDroneApiModule
    {
        private readonly QualityTypeProvider _qualityTypeProvider;

        public QualityTypeModule(QualityTypeProvider qualityTypeProvider)
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

            var type = Mapper.Map<QualityTypeModel, Core.Repository.Quality.QualityType>(model);
            _qualityTypeProvider.Update(type);

            return model.AsResponse();

        }

        private Response GetQualityType(int id)
        {
            var type = _qualityTypeProvider.Get(id);
            return Mapper.Map<Core.Repository.Quality.QualityType, QualityTypeModel>(type).AsResponse();
        }

        private Response GetQualityType()
        {
            var types = _qualityTypeProvider.All().Where(qualityType => qualityType.QualityTypeId != 0 && qualityType.QualityTypeId != 10).ToList();
            var responseModel = Mapper.Map<List<Core.Repository.Quality.QualityType>, List<QualityTypeModel>>(types);

            return responseModel.AsResponse();
        }
    }
}