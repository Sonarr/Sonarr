using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NzbDrone.Core.Datastore
{
    public class TableMapper
    {
        private readonly HashSet<string> _allowedOrderBy = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public TableMapper()
        {
            IgnoreList = new Dictionary<Type, List<PropertyInfo>>();
            LazyLoadList = new Dictionary<Type, List<LazyLoadedProperty>>();
            TableMap = new Dictionary<Type, string>();
        }

        public Dictionary<Type, List<PropertyInfo>> IgnoreList { get; set; }
        public Dictionary<Type, List<LazyLoadedProperty>> LazyLoadList { get; set; }
        public Dictionary<Type, string> TableMap { get; set; }

        public ColumnMapper<TEntity> Entity<TEntity>(string tableName)
            where TEntity : ModelBase
        {
            var type = typeof(TEntity);
            TableMap.Add(type, tableName);

            if (IgnoreList.TryGetValue(type, out var list))
            {
                return new ColumnMapper<TEntity>(list, LazyLoadList[type], _allowedOrderBy);
            }

            IgnoreList[type] = new List<PropertyInfo>();
            LazyLoadList[type] = new List<LazyLoadedProperty>();
            return new ColumnMapper<TEntity>(IgnoreList[type], LazyLoadList[type], _allowedOrderBy);
        }

        public List<PropertyInfo> ExcludeProperties(Type x)
        {
            return IgnoreList.ContainsKey(x) ? IgnoreList[x] : new List<PropertyInfo>();
        }

        public string TableNameMapping(Type x)
        {
            return TableMap.ContainsKey(x) ? TableMap[x] : null;
        }

        public string SelectTemplate(Type x)
        {
            return $"SELECT /**select**/ FROM {TableMap[x]} /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
        }

        public string DeleteTemplate(Type x)
        {
            return $"DELETE FROM {TableMap[x]} /**where**/";
        }

        public string PageCountTemplate(Type x)
        {
            return $"SELECT /**select**/ FROM {TableMap[x]} /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/";
        }

        public bool IsValidSortKey(string sortKey)
        {
            string table = null;

            if (sortKey.Contains('.'))
            {
                var split = sortKey.Split('.');
                if (split.Length != 2)
                {
                    return false;
                }

                table = split[0];
                sortKey = split[1];
            }

            if (table != null && !TableMap.Values.Contains(table, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!_allowedOrderBy.Contains(sortKey))
            {
                return false;
            }

            return true;
        }
    }

    public class LazyLoadedProperty
    {
        public PropertyInfo Property { get; set; }
        public ILazyLoaded LazyLoad { get; set; }
    }

    public class ColumnMapper<T>
    where T : ModelBase
    {
        private readonly List<PropertyInfo> _ignoreList;
        private readonly List<LazyLoadedProperty> _lazyLoadList;
        private readonly HashSet<string> _allowedOrderBy;

        public ColumnMapper(List<PropertyInfo> ignoreList, List<LazyLoadedProperty> lazyLoadList, HashSet<string> allowedOrderBy)
        {
            _ignoreList = ignoreList;
            _lazyLoadList = lazyLoadList;
            _allowedOrderBy = allowedOrderBy;
        }

        public ColumnMapper<T> AutoMapPropertiesWhere(Func<PropertyInfo, bool> predicate)
        {
            var properties = typeof(T).GetProperties();
            _ignoreList.AddRange(properties.Where(x => !predicate(x)));
            _allowedOrderBy.UnionWith(properties.Where(x => predicate(x)).Select(x => x.Name));

            return this;
        }

        public ColumnMapper<T> RegisterModel()
        {
            return AutoMapPropertiesWhere(x => x.IsMappableProperty());
        }

        public ColumnMapper<T> Ignore(Expression<Func<T, object>> property)
        {
            _ignoreList.Add(property.GetMemberName());
            return this;
        }

        public ColumnMapper<T> LazyLoad<TChild>(Expression<Func<T, LazyLoaded<TChild>>> property, Func<IDatabase, T, TChild> query, Func<T, bool> condition)
        {
            var lazyLoad = new LazyLoaded<T, TChild>(query, condition);

            var item = new LazyLoadedProperty
            {
                Property = property.GetMemberName(),
                LazyLoad = lazyLoad
            };

            _lazyLoadList.Add(item);

            return this;
        }

        public ColumnMapper<T> HasOne<TChild>(Expression<Func<T, LazyLoaded<TChild>>> portalExpression, Func<T, int> childIdSelector)
            where TChild : ModelBase
        {
            return LazyLoad(portalExpression,
                            (db, parent) =>
                            {
                                var id = childIdSelector(parent);
                                return db.Query<TChild>(new SqlBuilder().Where<TChild>(x => x.Id == id)).SingleOrDefault();
                            },
                            parent => childIdSelector(parent) > 0);
        }
    }
}
