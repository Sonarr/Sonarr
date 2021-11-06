using System;
using Microsoft.AspNetCore.Mvc;

namespace Sonarr.Http.REST.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestDeleteByIdAttribute : HttpDeleteAttribute
    {
        public RestDeleteByIdAttribute()
            : base("{id:int}")
        {
        }
    }
}
