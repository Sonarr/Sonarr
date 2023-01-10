using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.Middleware
{
    public class StartingUpMiddleware
    {
        private const string MESSAGE = "Sonarr is starting up, please try again later";
        private readonly RequestDelegate _next;
        private readonly IRuntimeInfo _runtimeInfo;

        public StartingUpMiddleware(RequestDelegate next, IRuntimeInfo runtimeInfo)
        {
            _next = next;
            _runtimeInfo = runtimeInfo;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_runtimeInfo.IsStarting)
            {
                var isJson = context.Request.IsApiRequest();
                var message = isJson ? STJson.ToJson(new { ErrorMessage = MESSAGE }) : MESSAGE;
                var bytes = Encoding.UTF8.GetBytes(message);

                context.Response.StatusCode = 503;
                context.Response.ContentType = isJson ? "application/json" : "text/plain";
                await context.Response.Body.WriteAsync(bytes);

                return;
            }

            await _next(context);
        }
    }
}
