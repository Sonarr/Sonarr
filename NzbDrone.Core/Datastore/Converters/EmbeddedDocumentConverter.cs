using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Converters
{
    public class EmbeddedDocumentConverter : IConverter
    {
        private readonly IJsonSerializer _serializer;

        public EmbeddedDocumentConverter(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

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

            return  _serializer.Deserialize(stringValue, map.FieldType);
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == null) return null;

            var json = _serializer.Serialize(clrValue);
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