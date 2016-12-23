using System;
using System.Collections.Concurrent;
using NLog;
using NzbDrone.Common.Cache;

namespace NzbDrone.Common.TPL
{
    public interface IRateLimitService
    {
        void WaitAndPulse(string key, TimeSpan interval);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly ConcurrentDictionary<string, DateTime> _rateLimitStore;
        private readonly Logger _logger;

        public RateLimitService(ICacheManager cacheManager, Logger logger)
        {
            _rateLimitStore = cacheManager.GetCache<ConcurrentDictionary<string, DateTime>>(GetType(), "rateLimitStore").Get("rateLimitStore", () => new ConcurrentDictionary<string, DateTime>());
            _logger = logger;
        }

        public void WaitAndPulse(string key, TimeSpan interval)
        {
            var waitUntil = _rateLimitStore.AddOrUpdate(key,
                (s) => DateTime.UtcNow + interval,
                (s,i) => new DateTime(Math.Max(DateTime.UtcNow.Ticks, i.Ticks), DateTimeKind.Utc) + interval);

            waitUntil -= interval;

            var delay = waitUntil - DateTime.UtcNow;

            if (delay.TotalSeconds > 0.0)
            {
                _logger.Trace("Rate Limit triggered, delaying '{0}' for {1:0.000} sec", key, delay.TotalSeconds);
                System.Threading.Thread.Sleep(delay);
            }
        }
    }
}
