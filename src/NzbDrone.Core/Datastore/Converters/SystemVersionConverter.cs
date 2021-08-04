using System;
using System.Data;
using Dapper;

namespace NzbDrone.Core.Datastore.Converters
{
    public class SystemVersionConverter : SqlMapper.TypeHandler<Version>
    {
        public override Version Parse(object value)
        {
            if (value is string version)
            {
                return Version.Parse((string)value);
            }

            return null;
        }

        public override void SetValue(IDbDataParameter parameter, Version value)
        {
            parameter.Value = value.ToString();
        }
    }
}
