using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Common
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();

            return source.Where(element => knownKeys.Add(keySelector(element)));
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
    }
}