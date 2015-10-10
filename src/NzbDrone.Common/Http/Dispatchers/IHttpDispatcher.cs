using System.Net;

namespace NzbDrone.Common.Http.Dispatchers
{
    public interface IHttpDispatcher
    {
        HttpResponse GetResponse(HttpRequest request, CookieContainer cookies);
    }
}
