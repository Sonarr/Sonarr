using Newtonsoft.Json;
using Workarr.Http;

namespace Workarr.Serializer.Newtonsoft.Json
{
    public class HttpUriConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else if (value is HttpUri)
            {
                writer.WriteValue((value as HttpUri).FullUri);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new HttpUri(reader.ReadAsString());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HttpUri);
        }
    }
}
