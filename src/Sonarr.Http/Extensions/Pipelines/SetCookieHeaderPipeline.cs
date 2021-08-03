using System;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;

namespace Sonarr.Http.Extensions.Pipelines
{
    public class SetCookieHeaderPipeline : IRegisterNancyPipeline
    {
        public int Order => 99;

        public void Register(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline((Action<NancyContext>)Handle);
        }

        private void Handle(NancyContext context)
        {
            if (context.Request.IsContentRequest() || context.Request.IsBundledJsRequest())
            {
                var authCookie = context.Response.Cookies.FirstOrDefault(c => c.Name == "SonarrAuth");

                if (authCookie != null)
                {
                    context.Response.Cookies.Remove(authCookie);
                }
            }
        }
    }
}
