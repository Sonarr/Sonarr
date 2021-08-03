using System;
using Marr.Data.Converters;
using Marr.Data.Mapping;
using Newtonsoft.Json;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Converters
{
    public class QualityIntConverter : JsonConverter, IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return Quality.Unknown;
            }

            var val = Convert.ToInt32(context.DbValue);

            return (Quality)val;
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

            if (clrValue as Quality == null)
            {
                throw new InvalidOperationException("Attempted to save a quality that isn't really a quality");
            }

            var quality = clrValue as Quality;
            return (int)quality;
        }

        public Type DbType => typeof(int);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quality);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = reader.Value;
            return (Quality)Convert.ToInt32(item);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToDB(value));
        }
    }
}
