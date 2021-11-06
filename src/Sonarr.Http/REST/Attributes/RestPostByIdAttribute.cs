using System;
using Microsoft.AspNetCore.Mvc;

namespace Sonarr.Http.REST.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPostByIdAttribute : HttpPostAttribute
    {
    }
}
