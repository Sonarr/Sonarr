using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NzbDrone.Common.Serializer
{
    public class STJVersionConverter : JsonConverter<Version>
    {
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            else
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    try
                    {
                        var v = new Version(reader.GetString());
                        return v;
                    }
                    catch (Exception)
                    {
                        throw new JsonException();
                    }
                }
                else
                {
                    throw new JsonException();
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
