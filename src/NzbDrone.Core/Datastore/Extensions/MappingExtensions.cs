using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Marr.Data;
using Marr.Data.Mapping;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Datastore.Extensions
{
    public static class MappingExtensions
    {
        public static ColumnMapBuilder<T> MapResultSet<T>(this FluentMappings.MappingsFluentEntity<T> mapBuilder)
            where T : ResultSet, new()
        {
            return mapBuilder
                .Columns
                .AutoMapPropertiesWhere(IsMappableProperty);
        }

        public static ColumnMapBuilder<T> RegisterDefinition<T>(this FluentMappings.MappingsFluentEntity<T> mapBuilder, string tableName = null)
            where T : ProviderDefinition, new()
        {
            return RegisterModel(mapBuilder, tableName).Ignore(c => c.ImplementationName);
        }

        public static ColumnMapBuilder<T> RegisterModel<T>(this FluentMappings.MappingsFluentEntity<T> mapBuilder, string tableName = null)
            where T : ModelBase, new()
        {
            return mapBuilder.Table.MapTable(tableName)
                             .Columns
                             .AutoMapPropertiesWhere(IsMappableProperty)
                             .PrefixAltNames(string.Format("{0}_", typeof(T).Name))
                             .For(c => c.Id)
                             .SetPrimaryKey()
                             .SetReturnValue()
                             .SetAutoIncrement();
        }

        public static RelationshipBuilder<T> AutoMapChildModels<T>(this ColumnMapBuilder<T> mapBuilder)
        {
            return mapBuilder.Relationships.AutoMapPropertiesWhere(m =>
                    m.MemberType == MemberTypes.Property &&
                    typeof(ModelBase).IsAssignableFrom(((PropertyInfo)m).PropertyType));
        }

        public static bool IsMappableProperty(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;

            if (propertyInfo == null)
            {
                return false;
            }

            if (!propertyInfo.IsReadable() || !propertyInfo.IsWritable())
            {
                return false;
            }

            if (propertyInfo.PropertyType.IsSimpleType() || MapRepository.Instance.TypeConverters.ContainsKey(propertyInfo.PropertyType))
            {
                return true;
            }

            return false;
        }

        public static List<TModel> QueryScalar<TModel>(this IDataMapper dataMapper, string sql)
        {
            return dataMapper.ExecuteReader(sql, reader => (TModel)Convert.ChangeType(reader.GetValue(0), typeof(TModel))).ToList();
        }
    }
}
