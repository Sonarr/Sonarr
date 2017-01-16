using System;
using System.Globalization;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Datastore.Converters
{
    public class TimeSpanConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.Parse(context.DbValue.ToString());
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            if (dbValue is TimeSpan)
            {
                return dbValue;
            }

            return TimeSpan.Parse(dbValue.ToString(), CultureInfo.InvariantCulture);
        }

        public object ToDB(object clrValue)
        {
            if (clrValue.ToString().IsNullOrWhiteSpace())
            {
                return null;
            }

            return ((TimeSpan)clrValue).ToString("c", CultureInfo.InvariantCulture);
        }

        public Type DbType { get; private set; }
    }
}