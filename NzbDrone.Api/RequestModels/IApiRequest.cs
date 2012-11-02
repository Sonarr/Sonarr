using System.Linq;

namespace NzbDrone.Api.RequestModels
{
    public interface IApiRequest
    {
        string ApiKey { get; set; }
    }
}