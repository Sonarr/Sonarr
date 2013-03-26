using System;
using System.Linq;
using System.Linq.Expressions;
using Marr.Data;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore
{
    public static class RelationshipExtensions
    {
        public static RelationshipBuilder<TParent> HasOne<TParent, TChild>(this ColumnMapBuilder<TParent> columnMapBuilder, Expression<Func<TParent, object>> portalExpression, Func<TParent, int> childIdSelector)
            where TParent : ModelBase
            where TChild : ModelBase
        {
            return columnMapBuilder.Relationships.AutoMapComplexTypeProperties<ILazyLoaded>()
                                   .For(portalExpression)
                                   .LazyLoad((db, parent) => db.Query<TChild>().Single(c => c.Id == childIdSelector(parent)));


        }

        public static RelationshipBuilder<TParent> HasMany<TParent, TChild>(this ColumnMapBuilder<TParent> columnMapBuilder, Expression<Func<TParent, object>> portalExpression, Func<TParent, int> childIdSelector)
            where TParent : ModelBase
            where TChild : ModelBase
        {
            return columnMapBuilder.Relationships.AutoMapComplexTypeProperties<ILazyLoaded>()
                                   .For(portalExpression)
                                   .LazyLoad((db, parent) => db.Query<TChild>().Where(c => c.Id == childIdSelector(parent)).ToList());


        }
    }
}