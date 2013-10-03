using System;
using System.Linq;
using Marr.Data.Mapping.Strategies;
using System.Reflection;
using System.Collections;

namespace Marr.Data.Mapping
{
    [Obsolete("This class is obsolete.  Please use the 'Mappings' class.")]
    public class MapBuilder
    {
        private bool _publicOnly;

        public MapBuilder()
            : this(true)
        { }

        public MapBuilder(bool publicOnly)
        {
            _publicOnly = publicOnly;
        }

        #region - Columns -

        /// <summary>
        /// Creates column mappings for the given type.
        /// Maps all properties except ICollection properties.
        /// </summary>
        /// <typeparam name="T">The type that is being built.</typeparam>
        /// <returns><see cref="ColumnMapCollection"/></returns>
        public ColumnMapBuilder<T> BuildColumns<T>()
        {
            return BuildColumns<T>(m => m.MemberType == MemberTypes.Property &&
                !typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType));
        }

        /// <summary>
        /// Creates column mappings for the given type.
        /// Maps all properties that are simple types (int, string, DateTime, etc).  
        /// ICollection properties are not included.
        /// </summary>
        /// <typeparam name="T">The type that is being built.</typeparam>
        /// <returns><see cref="ColumnMapCollection"/></returns>
        public ColumnMapBuilder<T> BuildColumnsFromSimpleTypes<T>()
        {
            return BuildColumns<T>(m => m.MemberType == MemberTypes.Property &&
                DataHelper.IsSimpleType((m as PropertyInfo).PropertyType) &&
                !typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType));
        }

        /// <summary>
        /// Creates column mappings for the given type.  
        /// Maps properties that are included in the include list.
        /// </summary>
        /// <typeparam name="T">The type that is being built.</typeparam>
        /// <param name="propertiesToInclude"></param>
        /// <returns><see cref="ColumnMapCollection"/></returns>
        public ColumnMapBuilder<T> BuildColumns<T>(params string[] propertiesToInclude)
        {
            return BuildColumns<T>(m =>
                m.MemberType == MemberTypes.Property &&
                propertiesToInclude.Contains(m.Name));
        }

        /// <summary>
        /// Creates column mappings for the given type.
        /// Maps all properties except the ones in the exclusion list.
        /// </summary>
        /// <typeparam name="T">The type that is being built.</typeparam>
        /// <param name="propertiesToExclude"></param>
        /// <returns><see cref="ColumnMapCollection"/></returns>
        public ColumnMapBuilder<T> BuildColumnsExcept<T>(params string[] propertiesToExclude)
        {
            return BuildColumns<T>(m => 
                m.MemberType == MemberTypes.Property &&
                !propertiesToExclude.Contains(m.Name));
        }

        /// <summary>
        /// Creates column mappings for the given type if they match the predicate.
        /// </summary>
        /// <typeparam name="T">The type that is being built.</typeparam>
        /// <param name="predicate">Determines whether a mapping should be created based on the member info.</param>
        /// <returns><see cref="ColumnMapConfigurator"/></returns>
        public ColumnMapBuilder<T> BuildColumns<T>(Func<MemberInfo, bool> predicate)
        {
            Type entityType = typeof(T);
            ConventionMapStrategy strategy = new ConventionMapStrategy(_publicOnly);
            strategy.ColumnPredicate = predicate;
            ColumnMapCollection columns = strategy.MapColumns(entityType);
            MapRepository.Instance.Columns[entityType] = columns;
            return new ColumnMapBuilder<T>(null, columns);
        }

        /// <summary>
        /// Creates a ColumnMapBuilder that starts out with no pre-populated columns.
        /// All columns must be added manually using the builder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ColumnMapBuilder<T> Columns<T>()
        {
            Type entityType = typeof(T);
            ColumnMapCollection columns = new ColumnMapCollection();
            MapRepository.Instance.Columns[entityType] = columns;
            return new ColumnMapBuilder<T>(null, columns);
        }
        
        #endregion

        #region - Relationships -

        /// <summary>
        /// Creates relationship mappings for the given type.
        /// Maps all properties that implement ICollection.
        /// </summary>
        /// <typeparam name="T">The type that is being built.</typeparam>
        /// <returns><see cref="RelationshipBuilder"/></returns>
        public RelationshipBuilder<T> BuildRelationships<T>()
        {
            return BuildRelationships<T>(m => 
                m.MemberType == MemberTypes.Property && 
                typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType));
        }

        /// <summary>
        /// Creates relationship mappings for the given type.
        /// Maps all properties that are listed in the include list.
        /// </summary>
        /// <typeparam name="T">The type that is being built.</typeparam>
        /// <param name="propertiesToInclude"></param>
        /// <returns><see cref="RelationshipBuilder"/></returns>
        public RelationshipBuilder<T> BuildRelationships<T>(params string[] propertiesToInclude)
        {
            Func<MemberInfo, bool> predicate = m => 
                (
                    // ICollection properties
                    m.MemberType == MemberTypes.Property && 
                    typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType) &&
                    propertiesToInclude.Contains(m.Name)
                ) || ( // Single entity properties
                    m.MemberType == MemberTypes.Property &&
                    !typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType) &&
                    propertiesToInclude.Contains(m.Name)
                );
            
            return BuildRelationships<T>(predicate);
        }

        /// <summary>
        /// Creates relationship mappings for the given type if they match the predicate.
        /// </summary>
        /// <typeparam name="T">The type that is being built.</typeparam>
        /// <param name="predicate">Determines whether a mapping should be created based on the member info.</param>
        /// <returns><see cref="RelationshipBuilder"/></returns>
        public RelationshipBuilder<T> BuildRelationships<T>(Func<MemberInfo, bool> predicate)
        {
            Type entityType = typeof(T);
            ConventionMapStrategy strategy = new ConventionMapStrategy(_publicOnly);
            strategy.RelationshipPredicate = predicate;
            RelationshipCollection relationships = strategy.MapRelationships(entityType);
            MapRepository.Instance.Relationships[entityType] = relationships;
            return new RelationshipBuilder<T>(null, relationships);
        }

        /// <summary>
        /// Creates a RelationshipBuilder that starts out with no pre-populated relationships.
        /// All relationships must be added manually using the builder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RelationshipBuilder<T> Relationships<T>()
        {
            Type entityType = typeof(T);
            RelationshipCollection relationships = new RelationshipCollection();
            MapRepository.Instance.Relationships[entityType] = relationships;
            return new RelationshipBuilder<T>(null, relationships);
        }
        
        #endregion

        #region - Tables -

        /// <summary>
        /// Provides a fluent table mapping interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TableBuilder<T> BuildTable<T>()
        {
            return new TableBuilder<T>(null);
        }

        /// <summary>
        /// Sets the table name for a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        public TableBuilder<T> BuildTable<T>(string tableName)
        {
            return new TableBuilder<T>(null).SetTableName(tableName);
        }

        #endregion
    }
}
