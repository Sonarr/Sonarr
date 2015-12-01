using System;
using Newtonsoft.Json;

namespace NzbDrone.Common.Serializer
{
    public class TimeSpanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var timespan = (TimeSpan)value;
                var format = timespan.Days > 0 ? @"dd\.hh\:mm\:ss" : @"hh\:mm\:ss";

                writer.WriteValue(timespan.ToString(format));
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!CanConvert(objectType))
            {
                throw new JsonSerializationException("Can't convert type " + existingValue.GetType().FullName + " to TimeSpan");
            }

            return TimeSpan.Parse(reader.Value.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (TimeSpan);
        }
    }
}