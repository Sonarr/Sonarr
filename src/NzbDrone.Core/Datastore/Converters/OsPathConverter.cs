using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Datastore.Converters
{
    public class OsPathConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var value = (string)context.DbValue;

            return new OsPath(value);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            var value = (OsPath)clrValue;

            return value.FullPath;
        }

        public Type DbType => typeof(string);
    }
}
