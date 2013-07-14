using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend
{
    public interface IAddCacheHeaders
    {
        void ToResponse(Request request, Response response);
    }

    public class AddCacheHeaders : IAddCacheHeaders
    {
        public void ToResponse(Request request, Response response)
        {
            if (!RuntimeInfo.IsProduction)
            {
                response.Headers.DisableCache();
                return;
            }

            if (request.Url.Path.ToLower() == "app.js")
            {
                response.Headers.DisableCache();
                return;
            }

            response.Headers.EnableCache();
        }
    }
}