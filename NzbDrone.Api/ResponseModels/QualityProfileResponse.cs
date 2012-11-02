using System.Linq;
using ServiceStack.ServiceInterface.ServiceModel;

namespace NzbDrone.Api.ResponseModels
{
    public class QualityProfileResponse : IApiResponse
    {
        public string Result { get; set; }
        ResponseStatus IApiResponse.ResponseStatus { get; set; }
        ResponseStatus IHasResponseStatus.ResponseStatus { get; set; }
    }
}