using System.IO;

namespace NzbDrone.Api.Frontend
{
    public interface IMapHttpRequestsToDisk
    {
        string Map(string resourceUrl);
    }

    public class StaticResourceMapper : IMapHttpRequestsToDisk
    {
        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar).ToLower();


            return Path.Combine(Directory.GetCurrentDirectory(), "ui", path);
        }
    }
}