using NzbDrone.Common.Cache;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Caching
{
    public class ClearExpiredCacheService : IExecute<ClearExpiredCacheCommand>
    {
        private readonly ICacheManager _cacheManager;

        public ClearExpiredCacheService(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public void Execute(ClearExpiredCacheCommand message)
        {
            _cacheManager.ClearExpired();
        }
    }
}
