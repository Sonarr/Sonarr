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
            pipelines.BeforeRequest.AddItemToStartOfPipeline((Func<NancyContext, Response>) Handle);
        }

        private Response Handle(NancyContext context)
        {

            if (_urlBase.IsNotNullOrWhiteSpace() && _urlBase != context.Request.Url.BasePath)
            {
                return new RedirectResponse($"{_urlBase}{context.Request.Path}{context.Request.Url.Query}");
            }

            return null;
        }
    }
}