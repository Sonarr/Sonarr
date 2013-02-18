using System.Linq;
using Nancy;

namespace NzbDrone.Api.FrontendModule
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get[@"/"] = x => View["NzbDrone.Backbone/index.html"];
        }
    }
}