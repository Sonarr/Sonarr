using System;
using System.IO;
using System.Linq;
using NzbDrone.Common;

namespace NzbDrone.Api.Frontend
{
    public class StaticResourceMapper : IMapHttpRequestsToDisk
    {
        private readonly IEnvironmentProvider _environmentProvider;
        private static readonly string[] Extensions = new[] { ".css", ".js", ".html", ".htm", ".jpg", ".jpeg", ".icon", ".gif", ".png", ".woff", ".ttf" };

        public StaticResourceMapper(IEnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
        }

        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar).ToLower();


            return Path.Combine(_environmentProvider.StartUpPath, "ui", path);
        }

        public bool CanHandle(string resourceUrl)
        {
            if (string.IsNullOrWhiteSpace(resourceUrl))
            {
                return false;
            }

            if (resourceUrl.StartsWith("/mediacover", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            return Extensions.Any(resourceUrl.EndsWith);
        }
    }
}