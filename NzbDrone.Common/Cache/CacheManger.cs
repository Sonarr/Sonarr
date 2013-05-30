using System;
using System.Collections.Generic;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Common.Cache
{
    public interface ICacheManger
    {
        ICached<T> GetCache<T>(Type host, string name);
        ICached<T> GetCache<T>(Type host);
        //ICollection<ICached<T>> Caches<T> { get;}
        void Clear();
        ICollection<ICached> Caches { get; }
    }

    public class CacheManger : ICacheManger
    {
        private readonly ICached<ICached> _cache;

        public CacheManger()
        {
            _cache = new Cached<ICached>();

        }

        public ICached<T> GetCache<T>(Type host)
        {
            Ensure.That(() => host).IsNotNull();
            return GetCache<T>(host, host.FullName);
        }


        public void Clear()
        {
            _cache.Clear();
        }

        public ICollection<ICached> Caches { get { return _cache.Values; } }

        public ICached<T> GetCache<T>(Type host, string name)
        {
            Ensure.That(() => host).IsNotNull();
            Ensure.That(() => name).IsNotNullOrWhiteSpace();

            return (ICached<T>)_cache.Get(host.FullName + "_" + name, () => new Cached<T>());
        }
    }
}