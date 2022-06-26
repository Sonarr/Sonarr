using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NzbDrone.Common.Serializer
{
    public static class STJson
    {
        private static readonly JsonSerializerOptions SerializerSettings = GetSerializerSettings();
        private static readonly JsonWriterOptions WriterOptions = new JsonWriterOptions
        {
            Indented = true
        };

        public static JsonSerializerOptions GetSerializerSettings()
        {
            var serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            serializerSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
            serializerSettings.Converters.Add(new STJVersionConverter());
            serializerSettings.Converters.Add(new STJHttpUriConverter());
            serializerSettings.Converters.Add(new STJTimeSpanConverter());
            serializerSettings.Converters.Add(new STJUtcConverter());

            return serializerSettings;
        }

        public static T Deserialize<T>(string json)
        where T : new()
        {
            return JsonSerializer.Deserialize<T>(json, SerializerSettings);
        }

        public static object Deserialize(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type, SerializerSettings);
        }

        public static object Deserialize(Stream input, Type type)
        {
            return JsonSerializer.DeserializeAsync(input, type, SerializerSettings).GetAwaiter().GetResult();
        }

        public static bool TryDeserialize<T>(string json, out T result)
        where T : new()
        {
            try
            {
                result = Deserialize<T>(json);
                return true;
            }
            catch (JsonException)
            {
                result = default(T);
                return false;
            }
        }

        public static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, SerializerSettings);
        }

        public static void Serialize<TModel>(TModel model, Stream outputStream, JsonSerializerOptions options = null)
        {
            if (options == null)
            {
                options = SerializerSettings;
            }

            // Cast to object to get all properties written out
            // https://github.com/dotnet/corefx/issues/38650
            using (var writer = new Utf8JsonWriter(outputStream, options: WriterOptions))
            {
                JsonSerializer.Serialize(writer, (object)model, options);
            }
        }
    }
}
