using System;
using Nancy;
using Nancy.Bootstrapper;
using Sonarr.Http.Frontend;

namespace Sonarr.Http.Extensions.Pipelines
{
    public class IfModifiedPipeline : IRegisterNancyPipeline
    {
        private readonly ICacheableSpecification _cacheableSpecification;

        public IfModifiedPipeline(ICacheableSpecification cacheableSpecification)
        {
            _cacheableSpecification = cacheableSpecification;
        }

        public int Order => 0;

        public void Register(IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline((Func<NancyContext, Response>) Handle);
        }

        private Response Handle(NancyContext context)
        {
            if (_cacheableSpecification.IsCacheable(context) && context.Request.Headers.IfModifiedSince.HasValue)
            {
                var response = new Response { ContentType = MimeTypes.GetMimeType(context.Request.Path), StatusCode = HttpStatusCode.NotModified };
                response.Headers.EnableCache();
                return response;
            }

            return null;
        }
    }
}