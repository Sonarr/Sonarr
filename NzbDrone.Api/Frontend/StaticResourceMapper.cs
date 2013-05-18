using System.IO;
using System.Linq;

namespace NzbDrone.Api.Frontend
{
    public class StaticResourceMapper : IMapHttpRequestsToDisk
    {
        private static readonly string[] Extensions = new[] { ".css", ".js", ".html", ".htm", ".jpg", ".jpeg", ".icon", ".gif", ".png", ".woff", ".ttf" };

        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar).ToLower();


            return Path.Combine("ui", path);
        }

        public bool CanHandle(string resourceUrl)
        {
            if (string.IsNullOrWhiteSpace(resourceUrl))
            {
                return false;
            }

            return Extensions.Any(resourceUrl.EndsWith);
        }
    }
}