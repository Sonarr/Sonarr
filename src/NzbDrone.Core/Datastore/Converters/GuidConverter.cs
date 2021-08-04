using System;
using System.Data;
using Dapper;

namespace NzbDrone.Core.Datastore.Converters
{
    public class GuidConverter : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            if (value == null)
            {
                return Guid.Empty;
            }

            return new Guid((string)value);
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }
    }
}
