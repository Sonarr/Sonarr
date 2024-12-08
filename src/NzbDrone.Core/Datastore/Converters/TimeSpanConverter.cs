using System;
using System.Data;
using Dapper;

namespace NzbDrone.Core.Datastore.Converters;

public class TimeSpanConverter : SqlMapper.TypeHandler<TimeSpan>
{
    public override void SetValue(IDbDataParameter parameter, TimeSpan value)
    {
        parameter.Value = value.ToString();
    }

    public override TimeSpan Parse(object value)
    {
        return value is string str ? TimeSpan.Parse(str) : TimeSpan.Zero;
    }
}
