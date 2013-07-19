using System;
using System.Linq;
using System.Linq.Expressions;
using Marr.Data.Mapping.Strategies;

namespace Marr.Data.Mapping
{
    /// <summary>
    /// This class has fluent methods that are used to easily configure relationship mappings.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class RelationshipBuilder<TEntity>
    {
        private FluentMappings.MappingsFluentEntity<TEntity> _fluentEntity;
        private string _currentPropertyName;

        public RelationshipBuilder(FluentMappings.MappingsFluentEntity<TEntity> fluentEntity, RelationshipCollection relationships)
        {
            _fluentEntity = fluentEntity;
            Relationships = relationships;
        }

        /// <summary>
        /// Gets the list of relationship mappings that are being configured.
        /// </summary>
        public RelationshipCollection Relationships { get; private set; }

        #region - Fluent Methods -

        /// <summary>
        /// Initializes the configurator to configure the given property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public RelationshipBuilder<TEntity> For(Expression<Func<TEntity, object>> property)
        {
            return For(property.GetMemberName());
        }

        /// <summary>
        /// Initializes the configurator to configure the given property or field.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public RelationshipBuilder<TEntity> For(string propertyName)
        {
            _currentPropertyName = propertyName;

            // Try to add the relationship if it doesn't exist
            if (Relationships[_currentPropertyName] == null)
            {
                TryAddRelationshipForField(_currentPropertyName);
            }

            return this;
        }

        /// <summary>
        /// Sets a property to be lazy loaded, with a given query.
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition">condition in which a child could exist. eg. avoid call to db if foreign key is 0 or null</param>
        /// <returns></returns>
        public RelationshipBuilder<TEntity> LazyLoad<TChild>(Func<IDataMapper, TEntity, TChild> query, Func<TEntity, bool> condition = null)
        {
            AssertCurrentPropertyIsSet();

            Relationships[_currentPropertyName].LazyLoaded = new LazyLoaded<TEntity, TChild>(query, condition);
            return this;
        }

        public RelationshipBuilder<TEntity> SetOneToOne()
        {
            AssertCurrentPropertyIsSet();
            SetOneToOne(_currentPropertyName);
            return this;
        }

        public RelationshipBuilder<TEntity> SetOneToOne(string propertyName)
        {
            Relationships[propertyName].RelationshipInfo.RelationType = RelationshipTypes.One;
            return this;
        }

        public RelationshipBuilder<TEntity> SetOneToMany()
        {
            AssertCurrentPropertyIsSet();
            SetOneToMany(_currentPropertyName);
            return this;
        }

        public RelationshipBuilder<TEntity> SetOneToMany(string propertyName)
        {
            Relationships[propertyName].RelationshipInfo.RelationType = RelationshipTypes.Many;
            return this;
        }

        public RelationshipBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> property)
        {
            string propertyName = property.GetMemberName();
            Relationships.RemoveAll(r => r.Member.Name == propertyName);
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

        public FluentMappings.MappingsFluentEntity<TNewEntity> Entity<TNewEntity>()
        {
            return new FluentMappings.MappingsFluentEntity<TNewEntity>(true);
        }

        /// <summary>
        /// Tries to add a Relationship for the given field name.  
        /// Throws and exception if field cannot be found.
        /// </summary>
        private void TryAddRelationshipForField(string fieldName)
        {
            // Set strategy to filter for public or private fields
            ConventionMapStrategy strategy = new ConventionMapStrategy(false);

            // Find the field that matches the given field name
            strategy.RelationshipPredicate = mi => mi.Name == fieldName;
            Relationship relationship = strategy.MapRelationships(typeof(TEntity)).FirstOrDefault();

            if (relationship == null)
            {
                throw new DataMappingException(string.Format("Could not find the field '{0}' in '{1}'.",
                    fieldName,
                    typeof(TEntity).Name));
            }
            else
            {
                Relationships.Add(relationship);
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
