using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ninject;
using NzbDrone.Api.Filters;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository.Quality;
using ServiceStack.ServiceInterface;

namespace NzbDrone.Api.QualityProfiles
{
    [ValidApiRequest]
    public class QualityProfileService : RestServiceBase<QualityProfileModel>
    {
        private readonly QualityProvider _qualityProvider;

        [Inject]
        public QualityProfileService(QualityProvider qualityProvider)
        {
            _qualityProvider = qualityProvider;
        }

        public QualityProfileService()
        {
        }

        public override object OnGet(QualityProfileModel request)
        {
            if (request.Id == 0)
            {
                var profiles = _qualityProvider.All();
                return Mapper.Map<List<QualityProfile>, List<QualityProfileModel>>(profiles);
            }

            var profile = _qualityProvider.Get(request.Id);
            return Mapper.Map<QualityProfile, QualityProfileModel>(profile);
        }

        //Create
        public override object OnPost(QualityProfileModel request)
        {
            var profile = Mapper.Map<QualityProfileModel, QualityProfile>(request);
            _qualityProvider.Add(profile);

            return request;
        }

        //Update
        public override object OnPut(QualityProfileModel request)
        {
            var profile = Mapper.Map<QualityProfileModel, QualityProfile>(request);
            _qualityProvider.Update(profile);

            return request;
        }

        public override object OnDelete(QualityProfileModel request)
        {
            _qualityProvider.Delete(request.Id);

            return request.Id.ToString();
        }
    }
}