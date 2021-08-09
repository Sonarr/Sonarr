using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Localization
{
    public class LocalizationResourceSerializer : JsonConverter<Dictionary<string, string>>
    {
        public override Dictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, string> dictionary, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var (key, value) in dictionary)
            {
                var propertyName = key;
                writer.WritePropertyName(propertyName);
                writer.WriteStringValue(value);
            }

            writer.WriteEndObject();
        }
    }

    public class LocalizationResource : RestResource
    {
        [JsonConverter(typeof(LocalizationResourceSerializer))]
        public Dictionary<string, string> Strings { get; set; }
    }

    public static class LocalizationResourceMapper
    {
        public static LocalizationResource ToResource(this Dictionary<string, string> localization)
        {
            if (localization == null)
            {
                return null;
            }

            return new LocalizationResource
            {
                Strings = localization,
            };
        }
    }
}
