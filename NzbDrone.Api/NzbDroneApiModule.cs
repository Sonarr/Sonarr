using System.Linq;
using Nancy;
using Nancy.Responses;

namespace NzbDrone.Api
{
    public abstract class NzbDroneApiModule : NancyModule
    {
        protected NzbDroneApiModule(string resource)
            : base("/api/" + resource.Trim('/'))
        {
            Options["/"] = x => new Response();
        }



    }
}