using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.HealthCheck;
using Sonarr.Http.REST;

namespace NzbDrone.Api.Health
{
    public class HealthResource : RestResource
    {
        public HealthCheckResult Type { get; set; }
        public string Message { get; set; }
        public HttpUri WikiUrl { get; set; }
    }

    public static class HealthResourceMapper
    {
        public static HealthResource ToResource(this HealthCheck model)
        {
            if (model == null) return null;

            return new HealthResource
            {
                Id = model.Id,

                Type = model.Type,
                Message = model.Message,
                WikiUrl = model.WikiUrl
            };
        }

        public static List<HealthResource> ToResource(this IEnumerable<HealthCheck> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
