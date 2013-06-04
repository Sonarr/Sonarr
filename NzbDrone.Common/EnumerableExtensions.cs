using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NzbDrone.Common
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TKey> SelectDistinct<TOuter, TKey>(this IEnumerable<TOuter> outer, Func<TOuter, TKey> outerKeySelector)
        {
            return outer.Select(outerKeySelector).Distinct();
        }

        public static IEnumerable<TOuter> Join<TOuter, TInner, TKey, TProperty>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Expression<Func<TOuter, TProperty>> outerProperty)
        {
            var outerList = outer.ToList();
            var innerList = inner.ToList();

            foreach (var outerElement in outerList)
            {
                var outerKey = outerKeySelector(outerElement);

                foreach (var innerElement in innerList)
                {
                    var innerKey = innerKeySelector(innerElement);

                    if (innerKey.Equals(outerKey))
                    {
                        var prop = (PropertyInfo)((MemberExpression)outerProperty.Body).Member;
                        prop.SetValue(outerElement, innerElement, null);
                    }
                }
            }

            return outerList;
        }
    }
}