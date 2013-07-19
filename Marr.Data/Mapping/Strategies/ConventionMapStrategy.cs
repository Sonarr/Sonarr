using System;
using System.Reflection;
using System.Collections;

namespace Marr.Data.Mapping.Strategies
{
    /// <summary>
    /// Allows you to specify a member based filter by defining predicates that filter the members that are mapped.
    /// </summary>
    public class ConventionMapStrategy : ReflectionMapStrategyBase
    {
        public ConventionMapStrategy(bool publicOnly)
            : base(publicOnly)
        {
            // Default: Only map members that are properties
            ColumnPredicate = m => m.MemberType == MemberTypes.Property;

            // Default: Only map members that are properties and that are ICollection types
            RelationshipPredicate = m =>
            {
                return m.MemberType == MemberTypes.Property && typeof(ICollection).IsAssignableFrom((m as PropertyInfo).PropertyType);
            };
        }

        public Func<MemberInfo, bool> ColumnPredicate;
        public Func<MemberInfo, bool> RelationshipPredicate;        

        

        protected override void CreateColumnMap(Type entityType, System.Reflection.MemberInfo member, ColumnAttribute columnAtt, ColumnMapCollection columnMaps)
        {
            if (ColumnPredicate(member))
            {
                // Map public property to DB column
                columnMaps.Add(new ColumnMap(member));
            }
        }

        protected override void CreateRelationship(Type entityType, System.Reflection.MemberInfo member, RelationshipAttribute relationshipAtt, RelationshipCollection relationships)
        {
            if (RelationshipPredicate(member))
            {
                relationships.Add(new Relationship(member));
            }
        }

    }
}
