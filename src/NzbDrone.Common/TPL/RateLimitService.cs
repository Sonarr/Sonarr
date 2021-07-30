using System;
using System.Collections.Concurrent;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.TPL
{
    public interface IRateLimitService
    {
        void WaitAndPulse(string key, TimeSpan interval);
        void WaitAndPulse(string key, string subKey, TimeSpan interval);
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
            WaitAndPulse(key, null, interval);
        }

        public void WaitAndPulse(string key, string subKey, TimeSpan interval)
        {
            var waitUntil = DateTime.UtcNow.Add(interval);

            if (subKey.IsNotNullOrWhiteSpace())
            {
                // Expand the base key timer, but don't extend it beyond now+interval.
                var baseUntil = _rateLimitStore.AddOrUpdate(key,
                    (s) => waitUntil,
                    (s, i) => new DateTime(Math.Max(waitUntil.Ticks, i.Ticks), DateTimeKind.Utc));

                if (baseUntil > waitUntil)
                {
                    waitUntil = baseUntil;
                }

                // Wait for the full key
                var combinedKey = key + "-" + subKey;
                waitUntil = _rateLimitStore.AddOrUpdate(combinedKey,
                    (s) => waitUntil,
                    (s, i) => new DateTime(Math.Max(waitUntil.Ticks, i.Add(interval).Ticks), DateTimeKind.Utc));
            }
            else
            {
                waitUntil = _rateLimitStore.AddOrUpdate(key,
                    (s) => waitUntil,
                    (s, i) => new DateTime(Math.Max(waitUntil.Ticks, i.Add(interval).Ticks), DateTimeKind.Utc));
            }

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
