using System;
using System.Linq;
using System.Linq.Expressions;

namespace NzbDrone.Core.Datastore.Extensions
{
    public static class PagingSpecExtensions
    {
        public static Expression<Func<TModel, object>> OrderByClause<TModel>(this PagingSpec<TModel> pagingSpec)
        {
            return CreateExpression<TModel>(pagingSpec.SortKey);
        }

        public static int PagingOffset<TModel>(this PagingSpec<TModel> pagingSpec)
        {
            return (pagingSpec.Page - 1) * pagingSpec.PageSize;
        }

        public static Marr.Data.QGen.SortDirection ToSortDirection<TModel>(this PagingSpec<TModel> pagingSpec)
        {
            if (pagingSpec.SortDirection == SortDirection.Descending)
            {
                return Marr.Data.QGen.SortDirection.Desc;
            }

            return Marr.Data.QGen.SortDirection.Asc;
        }

        private static Expression<Func<TModel, object>> CreateExpression<TModel>(string propertyName)
        {
            Type type = typeof(TModel);
            ParameterExpression parameterExpression = Expression.Parameter(type, "x");
            Expression expressionBody = parameterExpression;

            var splitPropertyName = propertyName.Split('.').ToList();

            foreach (var property in splitPropertyName)
            {
                expressionBody = Expression.Property(expressionBody, property);
            }

            expressionBody = Expression.Convert(expressionBody, typeof(object));
            return Expression.Lambda<Func<TModel, object>>(expressionBody, parameterExpression);
        }
    }
}
