using System.IO;

namespace NzbDrone.Api.Frontend
{
    public class StaticResourceMapper : IMapHttpRequestsToDisk
    {
        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar).ToLower();


            return Path.Combine("ui", path);
        }

        public RequestType IHandle { get { return RequestType.StaticResources; } }
    }
}