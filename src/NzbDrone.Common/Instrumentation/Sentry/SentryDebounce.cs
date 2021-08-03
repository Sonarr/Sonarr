using System;
using System.Collections.Generic;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    public class SentryDebounce
    {
        private readonly TimeSpan _ttl;
        private readonly Cached<bool> _cache;

        public SentryDebounce()
        {
            _cache = new Cached<bool>();
            _ttl = RuntimeInfo.IsProduction ? TimeSpan.FromHours(1) : TimeSpan.FromSeconds(10);
        }

        public bool Allowed(IEnumerable<string> fingerPrint)
        {
            var key = string.Join("|", fingerPrint);
            var exists = _cache.Find(key);

            if (exists)
            {
                return false;
            }

            _cache.Set(key, true, _ttl);
            return true;
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}
