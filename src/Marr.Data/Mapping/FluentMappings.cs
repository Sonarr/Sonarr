using System;
using System.Reflection;
using Marr.Data.Mapping.Strategies;
using System.Collections;

namespace Marr.Data.Mapping
{
    /// <summary>
    /// Provides a fluent interface for mapping domain entities and properties to database tables and columns.
    /// </summary>
    public class FluentMappings
    {
        private bool _publicOnly;

        public FluentMappings()
            : this(true)
        { }

        public FluentMappings(bool publicOnly)
        {
            _publicOnly = publicOnly;
            
        }

        public MappingsFluentEntity<TEntity> Entity<TEntity>()
        {
            return new MappingsFluentEntity<TEntity>(_publicOnly);
        }

        public class MappingsFluentEntity<TEntity>
        {
            public MappingsFluentEntity(bool publicOnly)
            {
                Columns = new MappingsFluentColumns<TEntity>(this, publicOnly);
                Table = new MappingsFluentTables<TEntity>(this);
                Relationships = new MappingsFluentRelationships<TEntity>(this, publicOnly);
            }

            /// <summary>
            /// Contains methods that map entity properties to database table and view column names;
            /// </summary>
            public MappingsFluentColumns<TEntity> Columns { get; private set; }

            /// <summary>
            /// Contains methods that map entity classes to database table names.
            /// </summary>
            public MappingsFluentTables<TEntity> Table { get; private set; }

            /// <summary>
            /// Contains methods that map sub-entities with database table and view column names.
            /// </summary>
            public MappingsFluentRelationships<TEntity> Relationships { get; private set; }
        }

        public class MappingsFluentColumns<TEntity>
        {
            private bool _publicOnly;
            private MappingsFluentEntity<TEntity> _fluentEntity;

            public MappingsFluentColumns(MappingsFluentEntity<TEntity> fluentEntity, bool publicOnly)
            {
                _fluentEntity = fluentEntity;
                _publicOnly = publicOnly;
            }

            /// <summary>
            /// Creates column mappings for the given type.
            /// Maps all properties except ICollection properties.
            /// </summary>
            /// <typeparam name="T">The type that is being built.</typeparam>
            /// <returns><see cref="ColumnMapCollection"/></returns>
            public ColumnMapBuilder<TEntity> AutoMapAllProperties()
            {
                return AutoMapPropertiesWhere(m => m.MemberType == MemberTypes.Property &&
                    !typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType));
            }

            /// <summary>
            /// Creates column mappings for the given type.
            /// Maps all properties that are simple types (int, string, DateTime, etc).  
            /// ICollection properties are not included.
            /// </summary>
            /// <typeparam name="T">The type that is being built.</typeparam>
            /// <returns><see cref="ColumnMapCollection"/></returns>
            public ColumnMapBuilder<TEntity> AutoMapSimpleTypeProperties()
            {
                return AutoMapPropertiesWhere(m => m.MemberType == MemberTypes.Property &&
                    DataHelper.IsSimpleType((m as PropertyInfo).PropertyType) &&
                    !typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType));
            }

            /// <summary>
            /// Creates column mappings for the given type if they match the predicate.
            /// </summary>
            /// <typeparam name="T">The type that is being built.</typeparam>
            /// <param name="predicate">Determines whether a mapping should be created based on the member info.</param>
            /// <returns><see cref="ColumnMapConfigurator"/></returns>
            public ColumnMapBuilder<TEntity> AutoMapPropertiesWhere(Func<MemberInfo, bool> predicate)
            {
                Type entityType = typeof(TEntity);
                ConventionMapStrategy strategy = new ConventionMapStrategy(_publicOnly);
                strategy.ColumnPredicate = predicate;
                ColumnMapCollection columns = strategy.MapColumns(entityType);
                MapRepository.Instance.Columns[entityType] = columns;
                return new ColumnMapBuilder<TEntity>(_fluentEntity, columns);
            }

            /// <summary>
            /// Creates a ColumnMapBuilder that starts out with no pre-populated columns.
            /// All columns must be added manually using the builder.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ColumnMapBuilder<TEntity> MapProperties()
            {
                Type entityType = typeof(TEntity);
                ColumnMapCollection columns = new ColumnMapCollection();
                MapRepository.Instance.Columns[entityType] = columns;
                return new ColumnMapBuilder<TEntity>(_fluentEntity, columns);
            }
        }

        public class MappingsFluentTables<TEntity>
        {
            private MappingsFluentEntity<TEntity> _fluentEntity;

            public MappingsFluentTables(MappingsFluentEntity<TEntity> fluentEntity)
            {
                _fluentEntity = fluentEntity;
            }

            /// <summary>
            /// Provides a fluent table mapping interface.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public TableBuilder<TEntity> AutoMapTable<T>()
            {
                return new TableBuilder<TEntity>(_fluentEntity);
            }

            /// <summary>
            /// Sets the table name for a given type.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="tableName"></param>
            public TableBuilder<TEntity> MapTable(string tableName)
            {
                return new TableBuilder<TEntity>(_fluentEntity).SetTableName(tableName);
            }
        }

        public class MappingsFluentRelationships<TEntity>
        {
            private MappingsFluentEntity<TEntity> _fluentEntity;
            private bool _publicOnly;

            public MappingsFluentRelationships(MappingsFluentEntity<TEntity> fluentEntity, bool publicOnly)
            {
                _fluentEntity = fluentEntity;
                _publicOnly = publicOnly;
            }

            /// <summary>
            /// Creates relationship mappings for the given type.
            /// Maps all properties that implement ICollection or are not "simple types".
            /// </summary>
            /// <returns></returns>
            public RelationshipBuilder<TEntity> AutoMapICollectionOrComplexProperties()
            {
                return AutoMapPropertiesWhere(m =>
                    m.MemberType == MemberTypes.Property &&
                    (
                        typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType) || !DataHelper.IsSimpleType((m as PropertyInfo).PropertyType)
                    )
                );

            }

            /// <summary>
            /// Creates relationship mappings for the given type.
            /// Maps all properties that implement ICollection.
            /// </summary>
            /// <returns><see cref="RelationshipBuilder"/></returns>
            public RelationshipBuilder<TEntity> AutoMapICollectionProperties()
            {
                return AutoMapPropertiesWhere(m =>
                    m.MemberType == MemberTypes.Property &&
                    typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType));
            }

            /// <summary>
            /// Creates relationship mappings for the given type.
            /// Maps all properties that are not "simple types".
            /// </summary>
            /// <returns></returns>
            public RelationshipBuilder<TEntity> AutoMapComplexTypeProperties<T>()
            {
                return AutoMapPropertiesWhere(m =>
                    m.MemberType == MemberTypes.Property &&
                    !DataHelper.IsSimpleType((m as PropertyInfo).PropertyType) &&
                    !MapRepository.Instance.TypeConverters.ContainsKey((m as PropertyInfo).PropertyType));
            }

            /// <summary>
            /// Creates relationship mappings for the given type if they match the predicate.
            /// </summary>
            /// <param name="predicate">Determines whether a mapping should be created based on the member info.</param>
            /// <returns><see cref="RelationshipBuilder"/></returns>
            public RelationshipBuilder<TEntity> AutoMapPropertiesWhere(Func<MemberInfo, bool> predicate)
            {
                Type entityType = typeof(TEntity);
                ConventionMapStrategy strategy = new ConventionMapStrategy(_publicOnly);
                strategy.RelationshipPredicate = predicate;
                RelationshipCollection relationships = strategy.MapRelationships(entityType);
                MapRepository.Instance.Relationships[entityType] = relationships;
                return new RelationshipBuilder<TEntity>(_fluentEntity, relationships);
            }

            /// <summary>
            /// Creates a RelationshipBuilder that starts out with no pre-populated relationships.
            /// All relationships must be added manually using the builder.
            /// </summary>
            /// <returns></returns>
            public RelationshipBuilder<TEntity> MapProperties<T>()
            {
                Type entityType = typeof(T);
                RelationshipCollection relationships = new RelationshipCollection();
                MapRepository.Instance.Relationships[entityType] = relationships;
                return new RelationshipBuilder<TEntity>(_fluentEntity, relationships);
            }
        }
    }
}
