using System;
using System.Globalization;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class UtcDateTimeConverter : IConverter
    {
        public Type DbType
        {
            get
            {
                return typeof(DateTime);
            }
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue != null && dbValue != DBNull.Value)
            {
                var dateTime = (DateTime)dbValue;
                dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Local);
                return dateTime.ToUniversalTime();
            }

            return null;
        }

        public object ToDB(object clrValue)
        {
            if (clrValue != null && clrValue != DBNull.Value)
            {
                return ((DateTime)clrValue).ToUniversalTime();
            }

            return DBNull.Value;
        }
    }
}