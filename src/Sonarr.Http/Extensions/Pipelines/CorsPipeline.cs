using System;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Extensions.Pipelines
{
    public class CorsPipeline : IRegisterNancyPipeline
    {
        public int Order => 0;

        public void Register(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(HandleRequest);
            pipelines.AfterRequest.AddItemToEndOfPipeline(HandleResponse);
        }

        private Response HandleRequest(NancyContext context)
        {
            if (context == null || context.Request.Method != "OPTIONS")
            {
                return null;
            }

            var response = new Response()
                .WithStatusCode(HttpStatusCode.OK)
                .WithContentType("");
            ApplyResponseHeaders(response, context.Request);
            return response;
        }

        private void HandleResponse(NancyContext context)
        {
            if (context == null || context.Response.Headers.ContainsKey(AccessControlHeaders.AllowOrigin))
            {
                return;
            }

            ApplyResponseHeaders(context.Response, context.Request);
        }

        private static void ApplyResponseHeaders(Response response, Request request)
        {
            if (request.IsApiRequest())
            {
                // Allow Cross-Origin access to the API since it's protected with the apikey, and nothing else.
                ApplyCorsResponseHeaders(response, request, "*", "GET, OPTIONS, PATCH, POST, PUT, DELETE");
            }
            else if (request.IsSharedContentRequest())
            {
                // Allow Cross-Origin access to specific shared content such as mediacovers and images.
                ApplyCorsResponseHeaders(response, request, "*", "GET, OPTIONS");
            }

            // Disallow Cross-Origin access for any other route.
        }

        private static void ApplyCorsResponseHeaders(Response response, Request request, string allowOrigin, string allowedMethods)
        {
            response.Headers.Add(AccessControlHeaders.AllowOrigin, allowOrigin);

            if (request.Method == "OPTIONS")
            {
                if (response.Headers.ContainsKey("Allow"))
                {
                    allowedMethods = response.Headers["Allow"];
                }

                response.Headers.Add(AccessControlHeaders.AllowMethods, allowedMethods);

                if (request.Headers[AccessControlHeaders.RequestHeaders].Any())
                {
                    var requestedHeaders = request.Headers[AccessControlHeaders.RequestHeaders].Join(", ");

                    response.Headers.Add(AccessControlHeaders.AllowHeaders, requestedHeaders);
                }
            }
        }
    }
}
