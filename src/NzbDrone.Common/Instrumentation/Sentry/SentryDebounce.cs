using System;
using System.Collections.Generic;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    public class SentryDebounce
    {
        private readonly Dictionary<string, DateTime> _dictionary;
        private static readonly TimeSpan TTL = TimeSpan.FromHours(1);

        public SentryDebounce()
        {
            _dictionary = new Dictionary<string, DateTime>();
        }

        public bool Allowed(IEnumerable<string> fingerPrint)
        {
            var key = string.Join("|", fingerPrint);

            DateTime expiry;
            _dictionary.TryGetValue(key, out expiry);

            if (expiry >= DateTime.Now)
            {
                return false;
            }

            _dictionary[key] = DateTime.Now + TTL;
            return true;
        }
    }
}