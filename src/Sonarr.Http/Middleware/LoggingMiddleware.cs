using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NLog;
using NzbDrone.Common.Extensions;
using Sonarr.Http.ErrorManagement;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.Middleware
{
    public class LoggingMiddleware
    {
        private static readonly Logger _loggerHttp = LogManager.GetLogger("Http");
        private static readonly Logger _loggerApi = LogManager.GetLogger("Api");
        private static int _requestSequenceID;

        private readonly SonarrErrorPipeline _errorHandler;
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next,
            SonarrErrorPipeline errorHandler)
        {
            _next = next;
            _errorHandler = errorHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            LogStart(context);

            await _next(context);

            LogEnd(context);
        }

        private void LogStart(HttpContext context)
        {
            var id = Interlocked.Increment(ref _requestSequenceID);

            context.Items["ApiRequestSequenceID"] = id;
            context.Items["ApiRequestStartTime"] = DateTime.UtcNow;

            var reqPath = GetRequestPathAndQuery(context.Request);

            _loggerHttp.Trace("Req: {0} [{1}] {2} (from {3})", id, context.Request.Method, reqPath, GetOrigin(context));
        }

        private void LogEnd(HttpContext context)
        {
            var id = (int)context.Items["ApiRequestSequenceID"];
            var startTime = (DateTime)context.Items["ApiRequestStartTime"];

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            var reqPath = GetRequestPathAndQuery(context.Request);

            _loggerHttp.Trace("Res: {0} [{1}] {2}: {3}.{4} ({5} ms)", id, context.Request.Method, reqPath, context.Response.StatusCode, (HttpStatusCode)context.Response.StatusCode, (int)duration.TotalMilliseconds);

            if (context.Request.IsApiRequest())
            {
                _loggerApi.Debug("[{0}] {1}: {2}.{3} ({4} ms)", context.Request.Method, reqPath, context.Response.StatusCode, (HttpStatusCode)context.Response.StatusCode, (int)duration.TotalMilliseconds);
            }
        }

        private static string GetRequestPathAndQuery(HttpRequest request)
        {
            if (request.QueryString.Value.IsNotNullOrWhiteSpace() && request.QueryString.Value != "?")
            {
                return string.Concat(request.Path, request.QueryString);
            }
            else
            {
                return request.Path;
            }
        }

        private static string GetOrigin(HttpContext context)
        {
            if (context.Request.Headers["UserAgent"].ToString().IsNullOrWhiteSpace())
            {
                return context.GetRemoteIP();
            }
            else
            {
                return $"{context.GetRemoteIP()} {context.Request.Headers["UserAgent"]}";
            }
        }
    }
}
