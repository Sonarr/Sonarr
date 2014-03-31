using Nancy;

namespace NzbDrone.Api
{
    public abstract class NzbDroneFeedModule : NancyModule
    {
        protected NzbDroneFeedModule(string resource)
            : base("/feed/" + resource.Trim('/'))
        {
        }
    }
}