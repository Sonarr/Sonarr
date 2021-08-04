using System;
using Newtonsoft.Json;

namespace NzbDrone.Common.Serializer
{
    public class IntConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!CanConvert(objectType))
            {
                throw new JsonSerializationException("Can't convert type " + existingValue.GetType().FullName + " to number");
            }

            if (objectType == typeof(long))
            {
                return Convert.ToInt64(reader.Value);
            }

            return Convert.ToInt32(reader.Value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int) || objectType == typeof(long) || objectType == typeof(int);
        }
    }
}
