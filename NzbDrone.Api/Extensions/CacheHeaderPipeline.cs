using Nancy;

namespace NzbDrone.Api.Extensions
{
    public static class CacheHeaderPipeline
    {
        public static void Handle(NancyContext context)
        {
            if (context.Response.ContentType.Contains("json"))
            {
                context.Response.Headers.DisableCache();

            }
        }
    }
}