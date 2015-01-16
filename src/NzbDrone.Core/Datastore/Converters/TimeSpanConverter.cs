using System;
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
                return DBNull.Value;
            }

            if (context.DbValue is TimeSpan)
            {
                return context.DbValue;
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

            return TimeSpan.Parse(dbValue.ToString());
        }

        public object ToDB(object clrValue)
        {
            if (clrValue.ToString().IsNullOrWhiteSpace())
            {
                return null;
            }

            return clrValue.ToString();
        }

        public Type DbType { get; private set; }
    }
}