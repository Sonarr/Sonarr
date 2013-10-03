
using Nancy;

namespace NzbDrone.Api.Frontend.Mappers
{
    public interface IMapHttpRequestsToDisk
    {
        bool CanHandle(string resourceUrl);
        Response GetResponse(string resourceUrl);
    }
}
