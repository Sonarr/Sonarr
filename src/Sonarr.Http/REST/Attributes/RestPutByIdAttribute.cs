using System;
using Microsoft.AspNetCore.Mvc;

namespace Sonarr.Http.REST.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPutByIdAttribute : HttpPutAttribute
    {
        public RestPutByIdAttribute()
            : base("{id:int?}")
        {
        }
    }
}
