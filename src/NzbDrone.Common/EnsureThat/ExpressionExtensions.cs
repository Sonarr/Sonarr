using System.Linq.Expressions;

namespace NzbDrone.Common.EnsureThat
{
    internal static class ExpressionExtensions
    {
        internal static string ToPath(this MemberExpression e)
        {
            var path = "";

            if (e.Expression is MemberExpression parent)
            {
                path = parent.ToPath() + ".";
            }

            return path + e.Member.Name;
        }

        internal static string GetPath(this Expression expression)
        {
            return GetRightMostMember(expression).ToPath();
        }

        private static MemberExpression GetRightMostMember(Expression e)
        {
            if (e is LambdaExpression)
            {
                return GetRightMostMember(((LambdaExpression)e).Body);
            }

            if (e is MemberExpression)
            {
                return (MemberExpression)e;
            }

            if (e is MethodCallExpression)
            {
                var callExpression = (MethodCallExpression)e;

                if (callExpression.Object is MethodCallExpression || callExpression.Object is MemberExpression)
                {
                    return GetRightMostMember(callExpression.Object);
                }

                var member = callExpression.Arguments.Count > 0 ? callExpression.Arguments[0] : callExpression.Object;
                return GetRightMostMember(member);
            }

            if (e is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)e;
                return GetRightMostMember(unaryExpression.Operand);
            }

            return null;
        }
    }
}
