using Sonarr.Http;

namespace Sonarr.Api.V3
{
    public abstract class SonarrV3FeedModule : SonarrModule
    {
        protected SonarrV3FeedModule(string resource)
            : base("/feed/v3/" + resource.Trim('/'))
        {
        }
    }
}
