using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class GuidConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return Guid.Empty;
            }

            var value = (string)context.DbValue;

            return new Guid(value);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            var value = clrValue;

            return value.ToString();
        }

        public Type DbType => typeof(string);
    }
}
