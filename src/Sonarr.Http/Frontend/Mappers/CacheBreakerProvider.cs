using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Frontend.Mappers
{
    public interface ICacheBreakerProvider
    {
        string AddCacheBreakerToPath(string resourceUrl);
    }

    public class CacheBreakerProvider : ICacheBreakerProvider
    {
        private readonly IEnumerable<IMapHttpRequestsToDisk> _diskMappers;
        private readonly IHashProvider _hashProvider;

        public CacheBreakerProvider(IEnumerable<IMapHttpRequestsToDisk> diskMappers, IHashProvider hashProvider)
        {
            _diskMappers = diskMappers;
            _hashProvider = hashProvider;
        }

        public string AddCacheBreakerToPath(string resourceUrl)
        {
            if (!ShouldBreakCache(resourceUrl))
            {
                return resourceUrl;
            }

            var mapper = _diskMappers.Single(m => m.CanHandle(resourceUrl));
            var pathToFile = mapper.Map(resourceUrl);
            var hash = _hashProvider.ComputeMd5(pathToFile).ToBase64();

            return resourceUrl + "?h=" + hash.Trim('=');
        }

        private static bool ShouldBreakCache(string path)
        {
            return !path.EndsWith(".ics") && !path.EndsWith("main");
        }
    }
}