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
                return Mapper.Map<IEnumerable<QualityProfile>, IEnumerable<QualityProfileModel>>(profiles);
            }

            var profile = _qualityProvider.Get(request.Id);
            return Mapper.Map<QualityProfile, QualityProfileModel>(profile);
        }

        public override object OnPost(QualityProfileModel data)
        {
            var profile = Mapper.Map<QualityProfileModel, QualityProfile>(data);
            _qualityProvider.Update(profile);

            return data;
        }

        public override object OnPut(QualityProfileModel data)
        {
            var profile = Mapper.Map<QualityProfileModel, QualityProfile>(data);
            data.Id = _qualityProvider.Add(profile);

            return data;
        }

        public override object OnDelete(QualityProfileModel data)
        {
            _qualityProvider.Delete(data.Id);

            return "ok";
        }
    }
}