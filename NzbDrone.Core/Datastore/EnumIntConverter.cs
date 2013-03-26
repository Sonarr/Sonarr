using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore
{
    public class EnumIntConverter : IConverter
    {
        public Type DbType
        {
            get
            {
                return typeof(int);
            }
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue != null && dbValue != DBNull.Value)
            {
                return Enum.ToObject(map.FieldType, (Int64)dbValue);
            }

            return null;
        }

        public object ToDB(object clrValue)
        {
            if (clrValue != null)
            {
                return (int)clrValue;
            }

            return DBNull.Value;
        }
    }
}