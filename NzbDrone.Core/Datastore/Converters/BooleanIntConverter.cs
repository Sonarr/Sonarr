using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class BooleanIntConverter : IConverter
    {
        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var val = (Int64)dbValue;

            switch (val)
            {
                case 1:
                    return true;
                case 0:
                    return false;
                default:
                    throw new ConversionException(string.Format("The BooleanCharConverter could not convert the value '{0}' to a Boolean.", dbValue));
            }
        }

        public object ToDB(object clrValue)
        {
            var val = (Nullable<bool>)clrValue;

            switch (val)
            {
                case true:
                    return 1;
                case false:
                    return 0;
                default:
                    return DBNull.Value;
            }
        }

        public Type DbType
        {
            get
            {
                return typeof(int);
            }
        }
    }
}