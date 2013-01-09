using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NzbDrone.Api.Filters;
using NzbDrone.Core.Providers;
using ServiceStack.ServiceInterface;

namespace NzbDrone.Api.QualityType
{
    [ValidApiRequest]
    public class QualityTypeService : RestServiceBase<QualityTypeModel>
    {
        private readonly QualityTypeProvider _qualityTypeProvider;

        public QualityTypeService(QualityTypeProvider qualityTypeProvider)
        {
            _qualityTypeProvider = qualityTypeProvider;
        }

        public QualityTypeService()
        {
        }

        public override object OnGet(QualityTypeModel request)
        {
            if (request.Id == 0)
            {
                var types = _qualityTypeProvider.All().Where(qualityType => qualityType.QualityTypeId != 0 && qualityType.QualityTypeId != 10).ToList();
                return Mapper.Map<List<Core.Repository.Quality.QualityType>, List<QualityTypeModel>>(types);
            }

            var type = _qualityTypeProvider.Get(request.Id);
            return Mapper.Map<Core.Repository.Quality.QualityType, QualityTypeModel>(type);
        }

        //Create
        public override object OnPost(QualityTypeModel request)
        {
            throw new NotImplementedException();
        }

        //Update
        public override object OnPut(QualityTypeModel request)
        {
            var type = Mapper.Map<QualityTypeModel, Core.Repository.Quality.QualityType>(request);
            _qualityTypeProvider.Update(type);

            return request;
        }

        public override object OnDelete(QualityTypeModel request)
        {
            throw new NotImplementedException();
        }
    }
}