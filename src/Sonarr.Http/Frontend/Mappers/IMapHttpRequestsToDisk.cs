using System.Threading.Tasks;
using Nancy;

namespace Sonarr.Http.Frontend.Mappers
{
    public interface IMapHttpRequestsToDisk
    {
        string Map(string resourceUrl);
        bool CanHandle(string resourceUrl);
        Task<Response> GetResponse(string resourceUrl);
    }
}
