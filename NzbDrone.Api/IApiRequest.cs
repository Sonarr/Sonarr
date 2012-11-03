using System.Linq;

namespace NzbDrone.Api
{
    public interface IApiRequest
    {
        string ApiKey { get; set; }
    }
}