using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class UtcConverter : IConverter
    {
        public object FromDB(ColumnMap map, object dbValue)
        {
            return dbValue;
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

        public Type DbType
        {
            get
            {
                return typeof(DateTime);
            }
        }
    }
}