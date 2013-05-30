using System;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common.Cache
{
    public interface ICacheManger
    {
        ICached<T> GetCache<T>(Type host, string name);
        ICached<T> GetCache<T>(Type host);
    }

    public class CacheManger : ICacheManger
    {
        private readonly ICached<object> _cache;

        public CacheManger()
        {
            _cache = new Cached<object>();

        }

        public ICached<T> GetCache<T>(Type host)
        {
            Ensure.That(() => host).IsNotNull();
            return GetCache<T>(host, host.FullName);
        }

        public ICached<T> GetCache<T>(Type host, string name)
        {
            Ensure.That(() => host).IsNotNull();
            Ensure.That(() => name).IsNotNullOrWhiteSpace();

            return (ICached<T>)_cache.Get(host.FullName + "_" + name, () => new Cached<T>());
        }
    }
}