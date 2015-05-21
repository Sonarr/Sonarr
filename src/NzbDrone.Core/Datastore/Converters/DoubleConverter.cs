using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class DoubleConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            if (context.DbValue is Double)
            {
                return context.DbValue;
            }

            return Convert.ToDouble(context.DbValue);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            if (dbValue is Double)
            {
                return dbValue;
            }

            return Convert.ToDouble(dbValue);
        }

        public object ToDB(object clrValue)
        {
            return clrValue;
        }

        public Type DbType { get; private set; }
    }
}
