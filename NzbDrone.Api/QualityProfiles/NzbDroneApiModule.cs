using System.Linq;
using Nancy;

namespace NzbDrone.Api.QualityProfiles
{
    public abstract class NzbDroneApiModule : NancyModule
    {
        protected NzbDroneApiModule(string resource)
                : base("/api/" + resource.Trim('/'))
        {
        }



    }
}