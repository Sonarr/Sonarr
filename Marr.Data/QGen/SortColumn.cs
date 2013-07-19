using System;
using System.Linq.Expressions;

namespace Marr.Data.QGen
{
    public class SortColumn<T>
    {
        public SortColumn(Expression<Func<T, object>> sortExpression, SortDirection direction)
        {
            MemberExpression me = GetMemberExpression(sortExpression.Body);
            DeclaringType = me.Expression.Type;
            PropertyName = me.Member.Name;
            Direction = direction;
        }

        public SortColumn(Type declaringType, string propertyName, SortDirection direction)
        {
            DeclaringType = declaringType;
            PropertyName = propertyName;
            Direction = direction;
        }

        public SortDirection Direction { get; private set; }
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
    }
    
    public enum SortDirection
    {
        Asc,
        Desc
    }
}
