using Sonarr.Http;

namespace NzbDrone.Api
{
    public abstract class NzbDroneFeedModule : SonarrModule
    {
        protected NzbDroneFeedModule(string resource)
            : base("/feed/" + resource.Trim('/'))
        {
        }
    }
}
