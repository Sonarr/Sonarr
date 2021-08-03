using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NzbDrone.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TFirst> IntersectBy<TFirst, TSecond, TKey>(this IEnumerable<TFirst> first,
            Func<TFirst, TKey> firstKeySelector,
            IEnumerable<TSecond> second,
            Func<TSecond, TKey> secondKeySelector,
            IEqualityComparer<TKey> keyComparer)
        {
            var keys = new HashSet<TKey>(second.Select(secondKeySelector), keyComparer);

            foreach (var element in first)
            {
                var key = firstKeySelector(element);

                // Remove the key so we only yield once
                if (keys.Remove(key))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TFirst> ExceptBy<TFirst, TSecond, TKey>(this IEnumerable<TFirst> first,
            Func<TFirst, TKey> firstKeySelector,
            IEnumerable<TSecond> second,
            Func<TSecond, TKey> secondKeySelector,
            IEqualityComparer<TKey> keyComparer)
        {
            var keys = new HashSet<TKey>(second.Select(secondKeySelector), keyComparer);
            var matchedKeys = new HashSet<TKey>();

            foreach (var element in first)
            {
                var key = firstKeySelector(element);

                if (!keys.Contains(key) && !matchedKeys.Contains(key))
                {
                    // Store the key so we only yield once
                    matchedKeys.Add(key);
                    yield return element;
                }
            }
        }

        public static Dictionary<TKey, TItem> ToDictionaryIgnoreDuplicates<TItem, TKey>(this IEnumerable<TItem> src, Func<TItem, TKey> keySelector)
        {
            var result = new Dictionary<TKey, TItem>();
            foreach (var item in src)
            {
                var key = keySelector(item);
                if (!result.ContainsKey(key))
                {
                    result[key] = item;
                }
            }

            return result;
        }

        public static Dictionary<TKey, TValue> ToDictionaryIgnoreDuplicates<TItem, TKey, TValue>(this IEnumerable<TItem> src, Func<TItem, TKey> keySelector, Func<TItem, TValue> valueSelector)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var item in src)
            {
                var key = keySelector(item);
                if (!result.ContainsKey(key))
                {
                    result[key] = valueSelector(item);
                }
            }

            return result;
        }

        public static void AddIfNotNull<TSource>(this List<TSource> source, TSource item)
        {
            if (item == null)
            {
                return;
            }

            source.Add(item);
        }

        public static bool Empty<TSource>(this IEnumerable<TSource> source)
        {
            return !source.Any();
        }

        public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return !source.Any(predicate);
        }

        public static bool NotAll<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return !source.All(predicate);
        }

        public static List<TResult> SelectList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> predicate)
        {
            return source.Select(predicate).ToList();
        }

//        public static IOrderedEnumerable<TEntity> OrderBy<TEntity>(this IEnumerable<TEntity> source, string propertyName, bool descending)
//        {
//            var property = typeof(TEntity).GetProperty(propertyName);
//            Func<TEntity, Object> orderByFunc = x => property.GetValue(x, null);
//
//            return descending ? source.OrderByDescending(orderByFunc) : source.OrderBy(orderByFunc);
//        }
        public static string ConcatToString<TSource>(this IEnumerable<TSource> source, string separator = ", ")
        {
            return string.Join(separator, source.Select(x => x.ToString()));
        }

        public static string ConcatToString<TSource>(this IEnumerable<TSource> source, Func<TSource, string> predicate, string separator = ", ")
        {
            return string.Join(separator, source.Select(predicate));
        }
    }
}
