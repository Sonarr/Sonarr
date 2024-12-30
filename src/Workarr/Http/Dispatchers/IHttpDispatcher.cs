using System.Net;

namespace Workarr.Http.Dispatchers
{
    public interface IHttpDispatcher
    {
        Task<HttpResponse> GetResponseAsync(HttpRequest request, CookieContainer cookies);
    }
}
