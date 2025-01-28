using System;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Sonarr.Http
{
    public class VersionedApiControllerAttribute : Attribute, IRouteTemplateProvider, IEnableCorsAttribute, IApiBehaviorMetadata
    {
        public const string API_CORS_POLICY = "ApiCorsPolicy";
        public const string CONTROLLER_RESOURCE = "[controller]";

        public VersionedApiControllerAttribute(int version, string resource = CONTROLLER_RESOURCE)
        {
            Resource = resource;
            Template = $"api/v{version}/{resource}";
            PolicyName = API_CORS_POLICY;
            Version = version;
        }

        public string Resource { get; }
        public string Template { get; }
        public int? Order => 2;
        public string Name { get; set; }
        public string PolicyName { get; set; }
        public int Version { get; set; }
    }

    public class V3ApiControllerAttribute : VersionedApiControllerAttribute
    {
        public V3ApiControllerAttribute(string resource = "[controller]")
            : base(3, resource)
        {
        }
    }

    public class V5ApiControllerAttribute : VersionedApiControllerAttribute
    {
        public V5ApiControllerAttribute(string resource = "[controller]")
            : base(5, resource)
        {
        }
    }
}
