using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class BooleanIntConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var val = (long)context.DbValue;

            switch (val)
            {
                case 1:
                    return true;
                case 0:
                    return false;
                default:
                    throw new ConversionException(string.Format("The BooleanCharConverter could not convert the value '{0}' to a Boolean.", context.DbValue));
            }
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            var val = (bool?)clrValue;

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

        public Type DbType => typeof(int);
    }
}
