using System;
using Nancy;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend
{
    public interface ICacheableSpecification
    {
        bool IsCacheable(NancyContext context);
    }

    public class CacheableSpecification : ICacheableSpecification
    {
        public bool IsCacheable(NancyContext context)
        {
            if (BuildInfo.IsDebug)
            {
                return false;
            }

            if (context.Request.Query.v == BuildInfo.Version) return true;

            if (context.Request.Path.StartsWith("/api", StringComparison.CurrentCultureIgnoreCase)) return false;
            if (context.Request.Path.StartsWith("/signalr", StringComparison.CurrentCultureIgnoreCase)) return false;
            if (context.Request.Path.EndsWith("app.js")) return false;

            if (context.Response != null)
            {
                if (context.Response.ContentType.Contains("text/html")) return false;
            }

            return true;
        }
    }
}