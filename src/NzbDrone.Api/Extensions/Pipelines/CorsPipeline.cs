using System;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;

namespace NzbDrone.Api.Extensions.Pipelines
{
    public class CorsPipeline : IRegisterNancyPipeline
    {
        public void Register(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(Handle);
        }

        private void Handle(NancyContext context)
        {
            if (context == null || context.Response.Headers.ContainsKey(AccessControlHeaders.AllowOrigin))
            {
                return;
            }

            ApplyResponseHeaders(context.Response, context.Request);
        }

        private static void ApplyResponseHeaders(Response response, Request request)
        {
            var allowedMethods = "GET, OPTIONS, PATCH, POST, PUT, DELETE";

            if (response.Headers.ContainsKey("Allow"))
            {
                allowedMethods = response.Headers["Allow"];
            }
            
            var requestedHeaders = String.Join(", ", request.Headers[AccessControlHeaders.RequestHeaders]);

            response.Headers.Add(AccessControlHeaders.AllowOrigin, "*");
            response.Headers.Add(AccessControlHeaders.AllowMethods, allowedMethods);

            if (request.Headers[AccessControlHeaders.RequestHeaders].Any())
            {
                response.Headers.Add(AccessControlHeaders.AllowHeaders, requestedHeaders);
            }
        }
    }
}
