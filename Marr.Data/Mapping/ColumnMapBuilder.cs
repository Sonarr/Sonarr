using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using Marr.Data.Mapping.Strategies;

namespace Marr.Data.Mapping
{
    /// <summary>
    /// This class has fluent methods that are used to easily configure column mappings.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ColumnMapBuilder<TEntity>
    {
        private FluentMappings.MappingsFluentEntity<TEntity> _fluentEntity;
        private string _currentPropertyName;

        public ColumnMapBuilder(FluentMappings.MappingsFluentEntity<TEntity> fluentEntity, ColumnMapCollection mappedColumns)
        {
            _fluentEntity = fluentEntity;
            MappedColumns = mappedColumns;
        }

        /// <summary>
        /// Gets the list of column mappings that are being configured.
        /// </summary>
        public ColumnMapCollection MappedColumns { get; private set; }

        #region - Fluent Methods -

        /// <summary>
        /// Initializes the configurator to configure the given property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public ColumnMapBuilder<TEntity> For(Expression<Func<TEntity, object>> property)
        {
            For(property.GetMemberName());
            return this;
        }

        /// <summary>
        /// Initializes the configurator to configure the given property or field.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public ColumnMapBuilder<TEntity> For(string propertyName)
        {
            _currentPropertyName = propertyName;

            // Try to add the column map if it doesn't exist
            if (MappedColumns.GetByFieldName(_currentPropertyName) == null)
            {
                TryAddColumnMapForField(_currentPropertyName);
            }

            return this;
        }

        public ColumnMapBuilder<TEntity> SetPrimaryKey()
        {
            AssertCurrentPropertyIsSet();
            return SetPrimaryKey(_currentPropertyName);
        }

        public ColumnMapBuilder<TEntity> SetPrimaryKey(string propertyName)
        {
            MappedColumns.GetByFieldName(propertyName).ColumnInfo.IsPrimaryKey = true;
            return this;
        }

        public ColumnMapBuilder<TEntity> SetAutoIncrement()
        {
            AssertCurrentPropertyIsSet();
            return SetAutoIncrement(_currentPropertyName);
        }

        public ColumnMapBuilder<TEntity> SetAutoIncrement(string propertyName)
        {
            MappedColumns.GetByFieldName(propertyName).ColumnInfo.IsAutoIncrement = true;
            return this;
        }

        public ColumnMapBuilder<TEntity> SetColumnName(string columnName)
        {
            AssertCurrentPropertyIsSet();
            return SetColumnName(_currentPropertyName, columnName);
        }

        public ColumnMapBuilder<TEntity> SetColumnName(string propertyName, string columnName)
        {
            MappedColumns.GetByFieldName(propertyName).ColumnInfo.Name = columnName;
            return this;
        }

        public ColumnMapBuilder<TEntity> SetReturnValue()
        {
            AssertCurrentPropertyIsSet();
            return SetReturnValue(_currentPropertyName);
        }

        public ColumnMapBuilder<TEntity> SetReturnValue(string propertyName)
        {
            MappedColumns.GetByFieldName(propertyName).ColumnInfo.ReturnValue = true;
            return this;
        }

        public ColumnMapBuilder<TEntity> SetSize(int size)
        {
            AssertCurrentPropertyIsSet();
            return SetSize(_currentPropertyName, size);
        }

        public ColumnMapBuilder<TEntity> SetSize(string propertyName, int size)
        {
            MappedColumns.GetByFieldName(propertyName).ColumnInfo.Size = size;
            return this;
        }

        public ColumnMapBuilder<TEntity> SetAltName(string altName)
        {
            AssertCurrentPropertyIsSet();
            return SetAltName(_currentPropertyName, altName);
        }

        public ColumnMapBuilder<TEntity> SetAltName(string propertyName, string altName)
        {
            MappedColumns.GetByFieldName(propertyName).ColumnInfo.AltName = altName;
            return this;
        }

        public ColumnMapBuilder<TEntity> SetParamDirection(ParameterDirection direction)
        {
            AssertCurrentPropertyIsSet();
            return SetParamDirection(_currentPropertyName, direction);
        }

        public ColumnMapBuilder<TEntity> SetParamDirection(string propertyName, ParameterDirection direction)
        {
            MappedColumns.GetByFieldName(propertyName).ColumnInfo.ParamDirection = direction;
            return this;
        }

        public ColumnMapBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> property)
        {
            string propertyName = property.GetMemberName();
            return Ignore(propertyName);
        }

        public ColumnMapBuilder<TEntity> Ignore(string propertyName)
        {
            var columnMap = MappedColumns.GetByFieldName(propertyName);
            MappedColumns.Remove(columnMap);
            return this;
        }

        public ColumnMapBuilder<TEntity> PrefixAltNames(string prefix)
        {
            MappedColumns.PrefixAltNames(prefix);
            return this;
        }

        public ColumnMapBuilder<TEntity> SuffixAltNames(string suffix)
        {
            MappedColumns.SuffixAltNames(suffix);
            return this;
        }

        public FluentMappings.MappingsFluentTables<TEntity> Tables
        {
            get
            {
                if (_fluentEntity == null)
                {
                    throw new Exception("This property is not compatible with the obsolete 'MapBuilder' class.");
                }

                return _fluentEntity.Table;
            }
        }

        public FluentMappings.MappingsFluentRelationships<TEntity> Relationships
        {
            get
            {
                if (_fluentEntity == null)
                {
                    throw new Exception("This property is not compatible with the obsolete 'MapBuilder' class.");
                }

                return _fluentEntity.Relationships;
            }
        }

        public FluentMappings.MappingsFluentEntity<TNewEntity> Entity<TNewEntity>()
        {
            return new FluentMappings.MappingsFluentEntity<TNewEntity>(true);
        }

        /// <summary>
        /// Tries to add a ColumnMap for the given field name.  
        /// Throws and exception if field cannot be found.
        /// </summary>
        private void TryAddColumnMapForField(string fieldName)
        {
            // Set strategy to filter for public or private fields
            ConventionMapStrategy strategy = new ConventionMapStrategy(false);

            // Find the field that matches the given field name
            strategy.ColumnPredicate = mi => mi.Name == fieldName;
            ColumnMap columnMap = strategy.MapColumns(typeof(TEntity)).FirstOrDefault();

            if (columnMap == null)
            {
                throw new DataMappingException(string.Format("Could not find the field '{0}' in '{1}'.",
                    fieldName,
                    typeof(TEntity).Name));
            }
            else
            {
                MappedColumns.Add(columnMap);
            }
        }

        /// <summary>
        /// Throws an exception if the "current" property has not been set.
        /// </summary>
        private void AssertCurrentPropertyIsSet()
        {
            if (string.IsNullOrEmpty(_currentPropertyName))
            {
                throw new DataMappingException("A property must first be specified using the 'For' method.");
            }
        }

        #endregion
    }
}
