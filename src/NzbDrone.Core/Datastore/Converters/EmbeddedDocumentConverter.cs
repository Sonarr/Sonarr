using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Datastore.Converters
{
    public class EmbeddedDocumentConverter : IConverter
    {
        public virtual object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var stringValue = (string)context.DbValue;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            return Json.Deserialize(stringValue, context.ColumnMap.FieldType);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == null) return null;

            return clrValue.ToJson();
        }

        public Type DbType
        {
            get
            {
                return typeof(string);
            }
        }
    }
}