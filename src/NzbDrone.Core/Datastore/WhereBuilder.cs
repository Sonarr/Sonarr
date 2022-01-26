using Dapper;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Datastore
{
    public abstract class WhereBuilder : ExpressionVisitor
    {
        public DynamicParameters Parameters { get; protected set; }
    }
}
