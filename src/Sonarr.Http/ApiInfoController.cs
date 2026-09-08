using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace NzbDrone.Http
{
    public class ApiInfoController : Controller
    {
        [HttpGet("/api")]
        [Produces("application/json")]
        public Ok<ApiInfoResource> GetApiInfo()
        {
            return TypedResults.Ok(new ApiInfoResource
            {
                Current = "v5",
                Deprecated = ["v3"]
            });
        }
    }
}
