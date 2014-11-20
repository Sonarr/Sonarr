using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Datastore.Converters
{
    public class OsPathConverter : IConverter
    {
        public Object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var value = (String)context.DbValue;

            return new OsPath(value);
        }

        public Object FromDB(ColumnMap map, Object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public Object ToDB(Object clrValue)
        {
            var value = (OsPath)clrValue;

            return value.FullPath;
        }

        public Type DbType
        {
            get { return typeof(String); }
        }
    }
}
