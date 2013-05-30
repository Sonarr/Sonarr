using System;
using System.Collections.Generic;

namespace NzbDrone.Common.Cache
{
    public interface ICached
    {
        bool ContainsKey(string key);
        void Clear();
        void Remove(string key);
    }

    public interface ICached<T> : ICached
    {
        void Set(string key, T value);
        T Get(string key, Func<T> function);
        T Get(string key);
        T Find(string key);

        ICollection<T> Values { get; }
        ICollection<string> Keys { get; }
    }
}