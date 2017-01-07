using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static Dictionary<T1, T2> Merge<T1, T2>(this Dictionary<T1, T2> first, Dictionary<T1, T2> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            var merged = new Dictionary<T1, T2>();
            first.ToList().ForEach(kv => merged[kv.Key] = kv.Value);
            second.ToList().ForEach(kv => merged[kv.Key] = kv.Value);

            return merged;
        }

        public static void Add<TKey, TValue>(this ICollection<KeyValuePair<TKey, TValue>> collection, TKey key, TValue value)
        {
            collection.Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }
}
