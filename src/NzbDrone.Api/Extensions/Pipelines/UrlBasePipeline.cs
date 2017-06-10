using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Extensions.Pipelines
{
    public class UrlBasePipeline : IRegisterNancyPipeline
    {
        private readonly string _urlBase;

        public UrlBasePipeline(IConfigFileProvider configFileProvider)
        {
            _urlBase = configFileProvider.UrlBase;
        }

        public int Order => 99;

        public void Register(IPipelines pipelines)
        {
            if (_urlBase.IsNotNullOrWhiteSpace())
            {
                pipelines.BeforeRequest.AddItemToStartOfPipeline((Func<NancyContext, Response>) Handle);
            }
        }

        private Response Handle(NancyContext context)
        {
            var basePath = context.Request.Url.BasePath;

            if (basePath.IsNullOrWhiteSpace())
            {
                return new RedirectResponse($"{_urlBase}{context.Request.Path}{context.Request.Url.Query}");
            }

            if (_urlBase != basePath)
            {
                return new NotFoundResponse();
            }

            return null;
        }
    }
}