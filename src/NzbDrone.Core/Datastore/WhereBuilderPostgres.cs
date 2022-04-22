using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;

namespace NzbDrone.Core.Datastore
{
    public class WhereBuilderPostgres : WhereBuilder
    {
        protected StringBuilder _sb;

        private const DbType EnumerableMultiParameter = (DbType)(-1);
        private readonly string _paramNamePrefix;
        private readonly bool _requireConcreteValue = false;
        private int _paramCount = 0;
        private bool _gotConcreteValue = false;

        public WhereBuilderPostgres(Expression filter, bool requireConcreteValue, int seq)
        {
            _paramNamePrefix = string.Format("Clause{0}", seq + 1);
            _requireConcreteValue = requireConcreteValue;
            _sb = new StringBuilder();

            Parameters = new DynamicParameters();

            if (filter != null)
            {
                Visit(filter);
            }
        }

        private string AddParameter(object value, DbType? dbType = null)
        {
            _gotConcreteValue = true;
            _paramCount++;
            var name = _paramNamePrefix + "_P" + _paramCount;
            Parameters.Add(name, value, dbType);
            return '@' + name;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            _sb.Append('(');

            Visit(expression.Left);

            _sb.AppendFormat(" {0} ", Decode(expression));

            Visit(expression.Right);

            _sb.Append(')');

            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            var method = expression.Method.Name;

            switch (expression.Method.Name)
            {
                case "Contains":
                    ParseContainsExpression(expression);
                    break;

                case "StartsWith":
                    ParseStartsWith(expression);
                    break;

                case "EndsWith":
                    ParseEndsWith(expression);
                    break;

                default:
                    var msg = string.Format("'{0}' expressions are not yet implemented in the where clause expression tree parser.", method);
                    throw new NotImplementedException(msg);
            }

            return expression;
        }

        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            var tableName = expression?.Expression?.Type != null ? TableMapping.Mapper.TableNameMapping(expression.Expression.Type) : null;
            var gotValue = TryGetRightValue(expression, out var value);

            // Only use the SQL condition if the expression didn't resolve to an actual value
            if (tableName != null && !gotValue)
            {
                _sb.Append($"\"{tableName}\".\"{expression.Member.Name}\"");
            }
            else
            {
                if (value != null)
                {
                    // string is IEnumerable<Char> but we don't want to pick up that case
                    var type = value.GetType();
                    var typeInfo = type.GetTypeInfo();
                    var isEnumerable =
                        type != typeof(string) && (
                            typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                            (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>)));

                    var paramName = isEnumerable ? AddParameter(value, EnumerableMultiParameter) : AddParameter(value);
                    _sb.Append(paramName);
                }
                else
                {
                    _gotConcreteValue = true;
                    _sb.Append("NULL");
                }
            }

            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression.Value != null)
            {
                var paramName = AddParameter(expression.Value);
                _sb.Append(paramName);
            }
            else
            {
                _gotConcreteValue = true;
                _sb.Append("NULL");
            }

            return expression;
        }

        private bool TryGetConstantValue(Expression expression, out object result)
        {
            result = null;

            if (expression is ConstantExpression constExp)
            {
                result = constExp.Value;
                return true;
            }

            return false;
        }

        private bool TryGetPropertyValue(MemberExpression expression, out object result)
        {
            result = null;

            if (expression.Expression is MemberExpression nested)
            {
                // Value is passed in as a property on a parent entity
                var container = (nested.Expression as ConstantExpression)?.Value;

                if (container == null)
                {
                    return false;
                }

                var entity = GetFieldValue(container, nested.Member);
                result = GetFieldValue(entity, expression.Member);
                return true;
            }

            return false;
        }

        private bool TryGetVariableValue(MemberExpression expression, out object result)
        {
            result = null;

            // Value is passed in as a variable
            if (expression.Expression is ConstantExpression nested)
            {
                result = GetFieldValue(nested.Value, expression.Member);
                return true;
            }

            return false;
        }

        private bool TryGetRightValue(Expression expression, out object value)
        {
            value = null;

            if (TryGetConstantValue(expression, out value))
            {
                return true;
            }

            var memberExp = expression as MemberExpression;

            if (TryGetPropertyValue(memberExp, out value))
            {
                return true;
            }

            if (TryGetVariableValue(memberExp, out value))
            {
                return true;
            }

            return false;
        }

        private object GetFieldValue(object entity, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                return (member as FieldInfo).GetValue(entity);
            }

            if (member.MemberType == MemberTypes.Property)
            {
                return (member as PropertyInfo).GetValue(entity);
            }

            throw new ArgumentException(string.Format("WhereBuilder could not get the value for {0}.{1}.", entity.GetType().Name, member.Name));
        }

        private bool IsNullVariable(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant &&
                TryGetConstantValue(expression, out var constResult) &&
                constResult == null)
            {
                return true;
            }

            if (expression.NodeType == ExpressionType.MemberAccess &&
                expression is MemberExpression member &&
                ((TryGetPropertyValue(member, out var result) && result == null) ||
                 (TryGetVariableValue(member, out result) && result == null)))
            {
                return true;
            }

            return false;
        }

        private string Decode(BinaryExpression expression)
        {
            if (IsNullVariable(expression.Right))
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Equal: return "IS";
                    case ExpressionType.NotEqual: return "IS NOT";
                }
            }

            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.And: return "AND";
                case ExpressionType.Equal: return "=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Or: return "OR";
                default: throw new NotSupportedException(string.Format("{0} statement is not supported", expression.NodeType.ToString()));
            }
        }

        private void ParseContainsExpression(MethodCallExpression expression)
        {
            var list = expression.Object;

            if (list != null && (list.Type == typeof(string)))
            {
                ParseStringContains(expression);
                return;
            }

            ParseEnumerableContains(expression);
        }

        private void ParseEnumerableContains(MethodCallExpression body)
        {
            // Fish out the list and the item to compare
            // It's in a different form for arrays and Lists
            var list = body.Object;
            Expression item;

            if (list != null)
            {
                // Generic collection
                item = body.Arguments[0];
            }
            else
            {
                // Static method
                // Must be Enumerable.Contains(source, item)
                if (body.Method.DeclaringType != typeof(Enumerable) || body.Arguments.Count != 2)
                {
                    throw new NotSupportedException("Unexpected form of Enumerable.Contains");
                }

                list = body.Arguments[0];
                item = body.Arguments[1];
            }

            _sb.Append('(');

            Visit(item);

            _sb.Append(" = ANY (");

            // hardcode the integer list if it exists to bypass parameter limit
            if (item.Type == typeof(int) && TryGetRightValue(list, out var value))
            {
                var items = (IEnumerable<int>)value;
                _sb.Append("('{");
                _sb.Append(string.Join(", ", items));
                _sb.Append("}')");

                _gotConcreteValue = true;
            }
            else
            {
                Visit(list);
            }

            _sb.Append("))");
        }

        private void ParseStringContains(MethodCallExpression body)
        {
            _sb.Append('(');

            Visit(body.Object);

            _sb.Append(" ILIKE '%' || ");

            Visit(body.Arguments[0]);

            _sb.Append(" || '%')");
        }

        private void ParseStartsWith(MethodCallExpression body)
        {
            _sb.Append('(');

            Visit(body.Object);

            _sb.Append(" ILIKE ");

            Visit(body.Arguments[0]);

            _sb.Append(" || '%')");
        }

        private void ParseEndsWith(MethodCallExpression body)
        {
            _sb.Append('(');

            Visit(body.Object);

            _sb.Append(" ILIKE '%' || ");

            Visit(body.Arguments[0]);

            _sb.Append(')');
        }

        public override string ToString()
        {
            var sql = _sb.ToString();

            if (_requireConcreteValue && !_gotConcreteValue)
            {
                var e = new InvalidOperationException("WhereBuilder requires a concrete condition");
                e.Data.Add("sql", sql);
                throw e;
            }

            return sql;
        }
    }
}
