using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace NzbDrone.Http
{
    public class ApiInfoController : Controller
    {
        [HttpGet("/api")]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Get information about the API.")]
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
