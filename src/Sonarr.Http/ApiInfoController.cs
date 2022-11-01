using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace NzbDrone.Http
{
    public class ApiInfoController : Controller
    {
        public ApiInfoController()
        {
        }

        [HttpGet("/api")]
        [Produces("application/json")]
        public object GetApiInfo()
        {
            return new ApiInfoResource
            {
                Current = "v3",
                Deprecated = new List<string>()
            };
        }
    }
}
