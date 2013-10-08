using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NzbDrone.Api.REST;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Api.Extensions
{
    public static class LazyExtensions
    {
        private static readonly ICached<MethodInfo> SetterCache = new Cached<MethodInfo>();

        public static IEnumerable<TParent> LoadSubtype<TParent, TChild>(this IEnumerable<TParent> parents, Func<TParent, int> foreignKeySelector, IBasicRepository<TChild> childRepository)
            where TChild : ModelBase, new()
            where TParent : RestResource
        {
            var parentList = parents.Where(p => foreignKeySelector(p) != 0).ToList();

            if (!parentList.Any())
            {
                return parents;
            }

            var ids = parentList.Select(foreignKeySelector).Distinct();
            var childDictionary = childRepository.Get(ids).ToDictionary(child => child.Id, child => child);

            var childSetter = GetChildSetter<TParent, TChild>();

            foreach (var episode in parentList)
            {
                childSetter.Invoke(episode, new object[] { childDictionary[foreignKeySelector(episode)] });
            }

            return parents;
        }


        private static MethodInfo GetChildSetter<TParent, TChild>()
            where TChild : ModelBase
            where TParent : RestResource
        {
            var key = typeof(TChild).FullName + typeof(TParent).FullName;

            return SetterCache.Get(key, () =>
                {
                    var property = typeof(TParent).GetProperties().Single(c => c.PropertyType == typeof(TChild));
                    return property.GetSetMethod();
                });
        }
    }
}
