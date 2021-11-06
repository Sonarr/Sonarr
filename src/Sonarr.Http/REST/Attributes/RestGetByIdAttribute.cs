using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Sonarr.Http.REST.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestGetByIdAttribute : Attribute, IActionHttpMethodProvider, IRouteTemplateProvider
    {
        public IEnumerable<string> HttpMethods => new[] { "GET" };
        public string Template => "{id:int}";
        public int? Order => 0;
        public string Name { get; }
    }
}
