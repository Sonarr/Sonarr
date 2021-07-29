using System;
using System.Threading;
using Nancy;
using Nancy.Bootstrapper;
using NLog;
using NzbDrone.Common.Extensions;
using Sonarr.Http.ErrorManagement;

namespace Sonarr.Http.Extensions.Pipelines
{
    public class RequestLoggingPipeline : IRegisterNancyPipeline
    {
        private static readonly Logger _loggerHttp = LogManager.GetLogger("Http");
        private static readonly Logger _loggerApi = LogManager.GetLogger("Api");

        private static int _requestSequenceID;

        private readonly SonarrErrorPipeline _errorPipeline;

        public RequestLoggingPipeline(SonarrErrorPipeline errorPipeline)
        {
            _errorPipeline = errorPipeline;
        }

        public int Order => 100;

        public void Register(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline(LogStart);
            pipelines.AfterRequest.AddItemToEndOfPipeline(LogEnd);
            pipelines.OnError.AddItemToEndOfPipeline(LogError);
        }

        private Response LogStart(NancyContext context)
        {
            var id = Interlocked.Increment(ref _requestSequenceID);

            context.Items["ApiRequestSequenceID"] = id;
            context.Items["ApiRequestStartTime"] = DateTime.UtcNow;

            var reqPath = GetRequestPathAndQuery(context.Request);

            _loggerHttp.Trace("Req: {0} [{1}] {2} (from {3})", id, context.Request.Method, reqPath, GetOrigin(context));

            return null;
        }

        private void LogEnd(NancyContext context)
        {
            var id = (int)context.Items["ApiRequestSequenceID"];
            var startTime = (DateTime)context.Items["ApiRequestStartTime"];

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            var reqPath = GetRequestPathAndQuery(context.Request);

            _loggerHttp.Trace("Res: {0} [{1}] {2}: {3}.{4} ({5} ms)", id, context.Request.Method, reqPath, (int)context.Response.StatusCode, context.Response.StatusCode, (int)duration.TotalMilliseconds);

            if (context.Request.IsApiRequest())
            {
                _loggerApi.Debug("[{0}] {1}: {2}.{3} ({4} ms)", context.Request.Method, reqPath, (int)context.Response.StatusCode, context.Response.StatusCode, (int)duration.TotalMilliseconds);
            }
        }

        private Response LogError(NancyContext context, Exception exception)
        {
            var response = _errorPipeline.HandleException(context, exception);

            context.Response = response;

            LogEnd(context);

            context.Response = null;

            return response;
        }

        private static string GetRequestPathAndQuery(Request request)
        {
            if (request.Url.Query.IsNotNullOrWhiteSpace())
            {
                return string.Concat(request.Url.Path, request.Url.Query);
            }
            else
            {
                return request.Url.Path;
            }
        }

        private static string GetOrigin(NancyContext context)
        {
            if (context.Request.Headers.UserAgent.IsNullOrWhiteSpace())
            {
                return context.GetRemoteIP();
            }
            else
            {
                return $"{context.GetRemoteIP()} {context.Request.Headers.UserAgent}";
            }
        }
    }
}