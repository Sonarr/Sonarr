using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace NzbDrone.Common.Serializer
{
    public static class Json
    {
        private static readonly JsonSerializer Serializer;
        private static readonly JsonSerializerSettings SerializerSettings;

        static Json()
        {
            SerializerSettings = GetSerializerSettings();
            Serializer = JsonSerializer.Create(SerializerSettings);
        }

        public static JsonSerializerSettings GetSerializerSettings()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            serializerSettings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
            serializerSettings.Converters.Add(new VersionConverter());
            serializerSettings.Converters.Add(new HttpUriConverter());

            return serializerSettings;
        }

        public static T Deserialize<T>(string json)
            where T : new()
        {
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
        }

        public static object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, SerializerSettings);
        }

        public static bool TryDeserialize<T>(string json, out T result)
            where T : new()
        {
            try
            {
                result = Deserialize<T>(json);
                return true;
            }
            catch (JsonReaderException)
            {
                result = default(T);
                return false;
            }
            catch (JsonSerializationException)
            {
                result = default(T);
                return false;
            }
        }

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, SerializerSettings);
        }

        public static void Serialize<TModel>(TModel model, TextWriter outputStream)
        {
            var jsonTextWriter = new JsonTextWriter(outputStream);
            Serializer.Serialize(jsonTextWriter, model);
            jsonTextWriter.Flush();
        }

        public static void Serialize<TModel>(TModel model, Stream outputStream)
        {
            Serialize(model, new StreamWriter(outputStream));
        }
    }
}
