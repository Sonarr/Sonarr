//http://fastjson.codeplex.com/

using System.Collections.Generic;

namespace Exceptron.Driver.fastJSON
{
    internal class SafeDictionary<TKey, TValue>
    {
        private readonly object _Padlock = new object();
        private readonly Dictionary<TKey, TValue> _Dictionary = new Dictionary<TKey, TValue>();


        public bool TryGetValue(TKey key, out TValue value)
        {
            return _Dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                return _Dictionary[key];
            }
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_Dictionary).GetEnumerator();
        }

        public void Add(TKey key, TValue value)
        {
            lock (_Padlock)
            {
                if (_Dictionary.ContainsKey(key) == false)
                    _Dictionary.Add(key, value);
            }
        }
    }
}
