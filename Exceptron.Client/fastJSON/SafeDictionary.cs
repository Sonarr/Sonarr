//http://fastjson.codeplex.com/
//http://fastjson.codeplex.com/license

using System.Collections.Generic;

namespace Exceptron.Client.fastJSON
{
    internal class SafeDictionary<TKey, TValue>
    {
        private readonly object _Padlock = new object();
        private readonly Dictionary<TKey, TValue> _Dictionary = new Dictionary<TKey, TValue>();


        internal bool TryGetValue(TKey key, out TValue value)
        {
            return _Dictionary.TryGetValue(key, out value);
        }

        internal TValue this[TKey key]
        {
            get
            {
                return _Dictionary[key];
            }
        }

        internal void Add(TKey key, TValue value)
        {
            lock (_Padlock)
            {
                if (_Dictionary.ContainsKey(key) == false)
                    _Dictionary.Add(key, value);
            }
        }
    }
}
