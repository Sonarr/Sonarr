using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Datastore.Converters
{
    public class LanguageIntConverter : JsonConverter, IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return Language.Unknown;
            }

            var val = Convert.ToInt32(context.DbValue);

            return (Language)val;
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == DBNull.Value)
            {
                return 0;
            }

            if (clrValue as Language == null)
            {
                throw new InvalidOperationException("Attempted to save a language that isn't really a language");
            }

            var language = clrValue as Language;
            return (int)language;
        }

        public Type DbType
        {
            get
            {
                return typeof(int);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Language);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = reader.Value;
            return (Language)Convert.ToInt32(item);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToDB(value));
        }
    }
}
