using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.Download.Clients.Sabnzbd.JsonConverters
{
    /// <summary>
    /// On some properties sab serializes array of single item as plain string.
    /// </summary>
    public class SabnzbdStringArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var stringArray = (string[])value;
            writer.WriteStartArray();

            for (var i = 0; i < stringArray.Length; i++)
            {
                writer.WriteValue(stringArray[i]);
            }

            writer.WriteEnd();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String || reader.TokenType == JsonToken.Null)
            {
                return new string[] { JValue.Load(reader).ToObject<string>() };
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                return JArray.Load(reader).ToObject<string[]>();
            }
            else
            {
                throw new JsonReaderException("Expected array");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string[]);
        }
    }
}
