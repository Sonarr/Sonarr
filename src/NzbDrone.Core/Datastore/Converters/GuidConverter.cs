using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class GuidConverter : IConverter
    {
        public Object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return Guid.Empty;
            }

            var value = (string)context.DbValue;

            return new Guid(value);
        }

        public Object FromDB(ColumnMap map, Object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public Object ToDB(Object clrValue)
        {
            var value = clrValue;

            return value.ToString();
        }

        public Type DbType
        {
            get { return typeof(string); }
        }
    }
}
