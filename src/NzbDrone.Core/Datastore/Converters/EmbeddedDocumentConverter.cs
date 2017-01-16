using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Datastore.Converters
{
    public class EmbeddedDocumentConverter : IConverter
    {
        private readonly JsonSerializerSettings SerializerSetting;

        public EmbeddedDocumentConverter(params JsonConverter[] converters)
        {
            SerializerSetting = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            SerializerSetting.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            SerializerSetting.Converters.Add(new VersionConverter());

            foreach (var converter in converters)
            {
                SerializerSetting.Converters.Add(converter);
            }
        }

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
            return JsonConvert.DeserializeObject(stringValue, context.ColumnMap.FieldType, SerializerSetting);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == null) return null;
            if (clrValue == DBNull.Value) return DBNull.Value;

            return JsonConvert.SerializeObject(clrValue, SerializerSetting);
        }

        public Type DbType => typeof(string);
    }
}