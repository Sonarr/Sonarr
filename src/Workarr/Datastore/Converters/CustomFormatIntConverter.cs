using System.Text.Json;
using System.Text.Json.Serialization;
using Workarr.CustomFormats;

namespace Workarr.Datastore.Converters
{
    public class CustomFormatIntConverter : JsonConverter<CustomFormat>
    {
        public override CustomFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new CustomFormat { Id = reader.GetInt32() };
        }

        public override void Write(Utf8JsonWriter writer, CustomFormat value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Id);
        }
    }
}
