using System.Linq;
using Ninject;
using NzbDrone.Api.Filters;
using NzbDrone.Core.Providers;
using ServiceStack.ServiceInterface;

namespace NzbDrone.Api.QualityProfiles
{
    [ValidApiRequest]
    public class QualityProfileService : RestServiceBase<QualityProfileRequest>
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

        public override object OnGet(QualityProfileRequest request)
        {
            if (request.Id == 0)
            {
                var profiles = _qualityProvider.All();
                return new { Profiles = profiles };
            }

            var profile = _qualityProvider.Get(request.Id);
            return profile;
        }

        //public override object OnPost(Todo todo)
        //{
        //    return Repository.Store(todo);
        //}

        //public override object OnPut(Todo todo)
        //{
        //    return Repository.Store(todo);
        //}

        //public override object OnDelete(Todo request)
        //{
        //    Repository.DeleteById(request.Id);
        //    return null;
        //}
    }
}