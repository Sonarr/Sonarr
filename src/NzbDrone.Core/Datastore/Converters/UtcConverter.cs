using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class UtcConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            return context.DbValue;
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == DBNull.Value)
            {
                return clrValue;
            }

            var dateTime = (DateTime)clrValue;
            return dateTime.ToUniversalTime();
        }

        public Type DbType => typeof(DateTime);
    }
}
