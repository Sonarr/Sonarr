using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Marr.Data.QGen
{
    public class SortColumn<T>
    {
        [Obsolete("Use ListSortDirection instead")]
        public SortColumn(Expression<Func<T, object>> sortExpression, SortDirection direction)
        {
            MemberExpression me = GetMemberExpression(sortExpression.Body);
            DeclaringType = me.Expression.Type;
            PropertyName = me.Member.Name;
            Direction = GetSortDirection(direction);
        }

        [Obsolete("Use ListSortDirection instead")]
        public SortColumn(Type declaringType, string propertyName, SortDirection direction)
        {
            DeclaringType = declaringType;
            PropertyName = propertyName;
            Direction = GetSortDirection(direction);
        }

        public SortColumn(Expression<Func<T, object>> sortExpression, ListSortDirection direction)
        {
            MemberExpression me = GetMemberExpression(sortExpression.Body);
            DeclaringType = me.Expression.Type;
            PropertyName = me.Member.Name;
            Direction = direction;
        }

        public SortColumn(Type declaringType, string propertyName, ListSortDirection direction)
        {
            DeclaringType = declaringType;
            PropertyName = propertyName;
            Direction = direction;
        }

        public ListSortDirection Direction { get; private set; }
        public Type DeclaringType { get; private set; }
        public string PropertyName { get; private set; }

        private MemberExpression GetMemberExpression(Expression exp)
        {
            MemberExpression me = exp as MemberExpression;

            if (me == null)
            {
                var ue = exp as UnaryExpression;
                me = ue.Operand as MemberExpression;
            }

            return me;
        }

        private ListSortDirection GetSortDirection(SortDirection direction)
        {
            if (direction == SortDirection.Desc) return ListSortDirection.Descending;
                
            return ListSortDirection.Ascending;
        }
    }
    
    public enum SortDirection
    {
        Asc,
        Desc
    }
}
