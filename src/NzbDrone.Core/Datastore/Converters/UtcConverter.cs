using System;
using System.Data;
using Dapper;

namespace NzbDrone.Core.Datastore.Converters
{
    public class DapperUtcConverter : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value.ToUniversalTime();
        }

        public override DateTime Parse(object value)
        {
            return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        }
    }
}
