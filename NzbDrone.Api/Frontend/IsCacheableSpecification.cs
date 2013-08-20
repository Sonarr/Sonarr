using System;
using Nancy;

namespace NzbDrone.Api.Frontend
{
    public class IsCacheableSpecification
    {
        public bool IsCacheable(Request request)
        {
            if (request.Path.Contains(".")) return false;
            if (request.Path.StartsWith("/api", StringComparison.CurrentCultureIgnoreCase)) return false;
            if (request.Path.StartsWith("/signalr", StringComparison.CurrentCultureIgnoreCase)) return false;


            return true;
        }
    }
}