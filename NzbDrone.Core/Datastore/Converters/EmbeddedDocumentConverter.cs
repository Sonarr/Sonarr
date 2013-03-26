using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;

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

            return JsonConvert.DeserializeObject(stringValue, map.FieldType);
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == null) return null;

            if (clrValue as IEmbeddedDocument == null)
            {
                throw new InvalidOperationException("Attempted to embedded an object not marked with IEmbeddedDocument");
            }

            var json = JsonConvert.SerializeObject(clrValue);
            return json;
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