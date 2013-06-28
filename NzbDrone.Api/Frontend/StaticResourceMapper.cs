using System;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend
{
    public class StaticResourceMapper : IMapHttpRequestsToDisk
    {
        private readonly IAppDirectoryInfo _appDirectoryInfo;
        private static readonly string[] Extensions = new[] { 
                                                              ".css",
                                                              ".js",
                                                              ".html",
                                                              ".htm",
                                                              ".jpg",
                                                              ".jpeg",
                                                              ".ico",
                                                              ".icon",
                                                              ".gif",
                                                              ".png",
                                                              ".woff",
                                                              ".ttf",
                                                              ".eot"
                                                            };

        public StaticResourceMapper(IAppDirectoryInfo appDirectoryInfo)
        {
            _appDirectoryInfo = appDirectoryInfo;
        }

        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar).ToLower();


            return Path.Combine(_appDirectoryInfo.StartUpPath, "ui", path);
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