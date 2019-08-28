using Sonarr.Http;

namespace NzbDrone.Api
{
    public abstract class NzbDroneApiModule : SonarrModule
    {
        protected NzbDroneApiModule(string resource)
            : base("/api/" + resource.Trim('/'))
        {
        }
    }
}
