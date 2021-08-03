using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace NzbDrone.Core.Datastore.Converters
{
    public class EmbeddedDocumentConverter : IConverter
    {
        protected readonly JsonSerializerSettings _serializerSetting;

        public EmbeddedDocumentConverter(params JsonConverter[] converters)
        {
            _serializerSetting = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            _serializerSetting.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
            _serializerSetting.Converters.Add(new VersionConverter());

            foreach (var converter in converters)
            {
                _serializerSetting.Converters.Add(converter);
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

            return JsonConvert.DeserializeObject(stringValue, context.ColumnMap.FieldType, _serializerSetting);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == null)
            {
                return null;
            }

            if (clrValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            return JsonConvert.SerializeObject(clrValue, _serializerSetting);
        }

        public Type DbType => typeof(string);
    }
}
