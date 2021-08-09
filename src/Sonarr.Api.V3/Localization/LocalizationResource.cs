using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Localization
{
    public class LocalizationResourceSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resource = value as LocalizationResource;
            writer.WriteStartObject();

            foreach (var r in resource.Strings)
            {
                writer.WritePropertyName(r.Key);
                serializer.Serialize(writer, r.Value);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(LocalizationResource).IsAssignableFrom(objectType);
        }
    }

    [JsonConverter(typeof(LocalizationResourceSerializer))]
    public class LocalizationResource : RestResource
    {
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
