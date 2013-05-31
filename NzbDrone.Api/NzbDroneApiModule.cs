using Nancy;

namespace NzbDrone.Api
{
    public abstract class NzbDroneApiModule : NancyModule
    {
        protected NzbDroneApiModule(string resource)
            : base("/api/" + resource.Trim('/'))
        {
        }
    }
}