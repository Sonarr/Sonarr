using System.Linq;
using ServiceStack.ServiceInterface.ServiceModel;

namespace NzbDrone.Api.ResponseModels
{
    public interface IApiResponse : IHasResponseStatus
    {
        string Result { get; set; }
        ResponseStatus ResponseStatus { get; set; } //Where Exceptions get auto-serialized
    }
}