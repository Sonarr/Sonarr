using Nancy;

namespace Sonarr.Api.V3
{
    public abstract class SonarrV3FeedModule : NancyModule
    {
        protected SonarrV3FeedModule(string resource)
            : base("/feed/v3/" + resource.Trim('/'))
        {
        }
    }
}