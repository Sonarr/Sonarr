using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NzbDrone.Core.Datastore
{
    public class IdService
    {
        private readonly IndexProvider _indexProvider;

        private static readonly ConcurrentDictionary<string, IList<PropertyInfo>> propertyCache = new ConcurrentDictionary<string, IList<PropertyInfo>>();

        public IdService(IndexProvider indexProvider)
        {
            _indexProvider = indexProvider;
        }

        public void EnsureIds<T>(T obj, HashSet<object> context)
        {
            //context is use to prevent infinite loop if objects are recursively looped.
            if (obj == null || context.Contains(obj))
            {
                return;
            }

            context.Add(obj);

            var modelBase = obj as BaseRepositoryModel;

            if (modelBase != null && modelBase.Id == 0)
            {
                modelBase.Id = _indexProvider.Next(obj.GetType());
            }

            foreach (var propertyInfo in GetPotentialProperties(obj.GetType()))
            {
                var propValue = propertyInfo.GetValue(obj, null);

                var list = propValue as IEnumerable;

                if (list != null)
                {
                    foreach (var item in list)
                    {
                        EnsureIds(item, context);
                    }
                }
                else
                {
                    EnsureIds(propValue, context);

                }

            }
        }

        private IList<PropertyInfo> GetPotentialProperties(Type type)
        {
            IList<PropertyInfo> result;
            if (!propertyCache.TryGetValue(type.FullName, out result))
            {
                result = type.GetProperties().Where(ShouldCrawl).ToList();
                propertyCache.TryAdd(type.FullName, result);
            }

            return result;
        }

        private bool ShouldCrawl(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead && ShouldCrawl(propertyInfo.PropertyType);
        }

        private bool ShouldCrawl(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArg = type.GetGenericArguments()[0];

                //skip if generic argument type isn't interesting
                if (!ShouldCrawl(genericArg))
                {
                    return false;
                }

                var listType = typeof(IList<>).MakeGenericType(genericArg);
                return listType.IsAssignableFrom(type);
            }

            return type.IsClass && type.FullName.StartsWith("NzbDrone");
        }
    }
}