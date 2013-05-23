using System;

namespace NzbDrone.Common.Cache
{
    public interface ICached<T>
    {
        void Set(string key, T value);
        T Get(string key, Func<T> function);
        bool ContainsKey(string key);
        void Clear();
        void Remove(string key);
        T Get(string key);
    }
}