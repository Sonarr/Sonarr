using System;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common.Cache
{
    public interface ICacheManger
    {
        ICached<T> GetCache<T>(Type type);
        ICached<T> GetCache<T>(object host);
    }

    public class CacheManger : ICacheManger
    {
        private readonly ICached<object> _cache;

        public CacheManger()
        {
            _cache = new Cached<object>();

        }

        public ICached<T> GetCache<T>(Type type)
        {
            Ensure.That(() => type).IsNotNull();

            return (ICached<T>)_cache.Get(type.FullName, () => new Cached<T>());
        }

        public ICached<T> GetCache<T>(object host)
        {
            return GetCache<T>(host.GetType());
        }
    }
}