using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Datastore.Converters
{
    public class EmbeddedDocumentConverter : IConverter
    {

        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var stringValue = (string)dbValue;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            return Json.Deserialize(stringValue, map.FieldType);
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