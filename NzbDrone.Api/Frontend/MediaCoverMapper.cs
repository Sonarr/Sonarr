using System.IO;
using NzbDrone.Common;

namespace NzbDrone.Api.Frontend
{
    public class MediaCoverMapper : IMapHttpRequestsToDisk
    {
        private readonly IEnvironmentProvider _environmentProvider;

        public MediaCoverMapper(IEnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
        }

        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar).ToLower();

            return Path.Combine(_environmentProvider.GetAppDataPath(), path);
        }

        public bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/mediacover");
        }
    }
}