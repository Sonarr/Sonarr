using System;
using System.Linq;
using System.Linq.Expressions;
using Marr.Data;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Extentions
{
    public static class RelationshipExtensions
    {
        public static RelationshipBuilder<TParent> HasOne<TParent, TChild>(this RelationshipBuilder<TParent> relationshipBuilder, Expression<Func<TParent, LazyLoaded<TChild>>> portalExpression, Func<TParent, int> childIdSelector)
            where TParent : ModelBase
            where TChild : ModelBase
        {
            return relationshipBuilder.For(portalExpression.GetMemberName())
                                .LazyLoad(
                                            query: (db, parent) => db.Query<TChild>().SingleOrDefault(c => c.Id == childIdSelector(parent)),
                                            condition: parent => childIdSelector(parent) > 0
                                         );

        }

        public static RelationshipBuilder<TParent> Relationship<TParent>(this ColumnMapBuilder<TParent> mapBuilder)
        {
            return mapBuilder.Relationships.AutoMapComplexTypeProperties<ILazyLoaded>();
        }



        public static RelationshipBuilder<TParent> HasMany<TParent, TChild>(this RelationshipBuilder<TParent> relationshipBuilder, Expression<Func<TParent, LazyList<TChild>>> portalExpression, Func<TParent, int> childIdSelector)
            where TParent : ModelBase
            where TChild : ModelBase
        {
            return relationshipBuilder.For(portalExpression.GetMemberName())
                   .LazyLoad((db, parent) => db.Query<TChild>().Where(c => c.Id == childIdSelector(parent)).ToList());


        }

        private static string GetMemberName<T, TMember>(this Expression<Func<T, TMember>> member)
        {
            var expression = member.Body as MemberExpression;

            if (expression == null)
            {
                expression = (MemberExpression)((UnaryExpression)member.Body).Operand;
            }

            return expression.Member.Name;
        }
    }
}