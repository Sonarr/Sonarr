using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Dapper;

namespace NzbDrone.Core.Datastore;

public abstract class WhereBuilder : ExpressionVisitor
{
    public DynamicParameters Parameters { get; protected set; }

    protected static bool TryUnwrapSpanImplicitCast(Expression expression, [NotNullWhen(true)] out Expression result)
    {
        if (expression is MethodCallExpression { Method: { Name: "op_Implicit", DeclaringType: { IsGenericType: true } implicitCastDeclaringType }, Arguments: [var unwrapped] }
            && implicitCastDeclaringType.GetGenericTypeDefinition() is var genericTypeDefinition
            && (genericTypeDefinition == typeof(Span<>) || genericTypeDefinition == typeof(ReadOnlySpan<>)))
        {
            result = unwrapped;
            return true;
        }

        result = null;
        return false;
    }
}
