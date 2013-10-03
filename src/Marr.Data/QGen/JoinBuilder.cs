using System;
using System.Data.Common;
using Marr.Data.QGen.Dialects;
using System.Linq.Expressions;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class overrides the WhereBuilder which utilizes the ExpressionVisitor base class, 
    /// and it is responsible for translating the lambda expression into a "JOIN ON" clause.
    /// It populates the protected string builder, which outputs the "JOIN ON" clause when the ToString method is called.
    /// </summary>
    /// <typeparam name="T">The entity that is on the left side of the join.</typeparam>
    /// <typeparam name="T2">The entity that is on the right side of the join.</typeparam>
    public class JoinBuilder<T, T2> : WhereBuilder<T>
    {
        public JoinBuilder(DbCommand command, Dialect dialect, Expression<Func<T, T2, bool>> filter, TableCollection tables)
            : base(command, dialect, filter.Body, tables, false, true)
        { }

        protected override string PrefixText
        {
            get
            {
                return "ON";
            }
        }

        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            string fqColumn = GetFullyQualifiedColumnName(expression.Member, expression.Expression.Type);
            _sb.Append(fqColumn);
            return expression;
        }
    }
}
