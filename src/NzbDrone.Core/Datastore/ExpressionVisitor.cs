/* This class was copied from Mehfuz's LinqExtender project, which is available from github.
 * http://mehfuzh.github.com/LinqExtender/
 */

using System;
using System.Linq.Expressions;

namespace NzbDrone.Core.Datastore
{
    /// <summary>
    /// Expression visitor
    /// </summary>
    public class ExpressionVisitor
    {
        /// <summary>
        /// Visits expression and delegates call to different to branch.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLamda((LambdaExpression)expression);
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)expression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return VisitBinary((BinaryExpression)expression);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)expression);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)expression);
            }

            throw new ArgumentOutOfRangeException("expression", expression.NodeType.ToString());
        }

        /// <summary>
        /// Visits the constance expression. To be implemented by user.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression VisitConstant(ConstantExpression expression)
        {
            return expression;
        }

        /// <summary>
        /// Visits the memeber access expression. To be implemented by user.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression VisitMemberAccess(MemberExpression expression)
        {
            return expression;
        }

        /// <summary>
        /// Visits the method call expression. To be implemented by user.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits the binary expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression VisitBinary(BinaryExpression expression)
        {
            Visit(expression.Left);
            Visit(expression.Right);
            return expression;
        }

        /// <summary>
        /// Visits the unary expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression VisitUnary(UnaryExpression expression)
        {
            Visit(expression.Operand);
            return expression;
        }

        /// <summary>
        /// Visits the lamda expression.
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        protected virtual Expression VisitLamda(LambdaExpression lambdaExpression)
        {
            Visit(lambdaExpression.Body);
            return lambdaExpression;
        }

        private Expression VisitParameter(ParameterExpression expression)
        {
            return expression;
        }
    }
}
