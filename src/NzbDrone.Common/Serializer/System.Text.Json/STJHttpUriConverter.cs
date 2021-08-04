using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NzbDrone.Common.Http;

namespace NzbDrone.Common.Serializer
{
    public class STJHttpUriConverter : JsonConverter<HttpUri>
    {
        public override HttpUri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new HttpUri(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, HttpUri value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.FullUri);
            }
        }
    }
}
