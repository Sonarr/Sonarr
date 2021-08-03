using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;

namespace NzbDrone.Core.Datastore.Converters
{
    public class SystemVersionConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue is string version)
            {
                return Version.Parse(version);
            }

            return null;
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue is string version)
            {
                return Version.Parse(version);
            }

            return null;
        }

        public object ToDB(object clrValue)
        {
            if (clrValue is Version version)
            {
                return version.ToString();
            }

            return DBNull.Value;
        }

        public Type DbType => typeof(string);
    }
}
