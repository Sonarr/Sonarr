using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace NzbDrone.Http
{
    public class ApiInfoController : Controller
    {
        [HttpGet("/api")]
        [Produces("application/json")]
        public object GetApiInfo()
        {
            return new ApiInfoResource
            {
                Current = "v5",
                Deprecated = new List<string> { "v3" }
            };
        }
    }
}
