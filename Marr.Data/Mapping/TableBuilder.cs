using System;

namespace Marr.Data.Mapping
{
    /// <summary>
    /// This class has fluent methods that are used to easily configure the table mapping.
    /// </summary>
    public class TableBuilder<TEntity>
    {
        private FluentMappings.MappingsFluentEntity<TEntity> _fluentEntity;

        public TableBuilder(FluentMappings.MappingsFluentEntity<TEntity> fluentEntity)
        {
            _fluentEntity = fluentEntity;
        }

        #region - Fluent Methods -

        public TableBuilder<TEntity> SetTableName(string tableName)
        {
            MapRepository.Instance.Tables[typeof(TEntity)] = tableName;
            return this;
        }

        public FluentMappings.MappingsFluentColumns<TEntity> Columns
        {
            get
            {
                if (_fluentEntity == null)
                {
                    throw new Exception("This property is not compatible with the obsolete 'MapBuilder' class.");
                }

                return _fluentEntity.Columns;
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

        #endregion
    }
}
