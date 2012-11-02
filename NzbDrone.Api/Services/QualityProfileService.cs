using System.Linq;
using NzbDrone.Api.Filters;
using NzbDrone.Api.RequestModels;
using NzbDrone.Api.ResponseModels;
using ServiceStack.ServiceInterface;

namespace NzbDrone.Api.Services
{
    [ValidApiRequest]
    public class QualityProfileService : RestServiceBase<QualityProfileRequest>
    {
        public override object OnGet(QualityProfileRequest request)
        {
            return new QualityProfileResponse { Result = "Your API Key is: " + request.ApiKey };
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