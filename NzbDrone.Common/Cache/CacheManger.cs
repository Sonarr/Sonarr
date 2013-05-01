using System;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common.Cache
{
    public static class CacheManger
    {
        private static readonly ICached<object> Cache;

        static CacheManger()
        {
            Cache = new Cached<object>();
        }

        public static ICached<T> GetCache<T>(Type type)
        {
            Ensure.That(() => type).IsNotNull();

            return (ICached<T>)Cache.Get(type.FullName, () => new Cached<T>());
        }
    }
}