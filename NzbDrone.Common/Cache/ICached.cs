using System;
using System.Collections.Generic;

namespace NzbDrone.Common.Cache
{
    public interface ICached
    {
        void Clear();
    }

    public interface ICached<T> : ICached
    {
        void Set(string key, T value, TimeSpan? lifetime = null);
        T Get(string key, Func<T> function, TimeSpan? lifeTime = null);
        T Find(string key);
        T Remove(string key);

        ICollection<T> Values { get; }
    }
}