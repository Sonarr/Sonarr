using System;
using System.IO;
using System.Reflection;
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
            try
            {
                return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
            }
            catch (JsonReaderException ex)
            {
                throw DetailedJsonReaderException(ex, json);
            }
        }

        public static object Deserialize(string json, Type type)
        {
            try
            {
                return JsonConvert.DeserializeObject(json, type, SerializerSettings);
            }
            catch (JsonReaderException ex)
            {
                throw DetailedJsonReaderException(ex, json);
            }
        }

        private static JsonReaderException DetailedJsonReaderException(JsonReaderException ex, string json)
        {
            var lineNumber = ex.LineNumber == 0 ? 0 : (ex.LineNumber - 1);
            var linePosition = ex.LinePosition;

            var lines = json.Split('\n');
            if (lineNumber >= 0 && lineNumber < lines.Length &&
                linePosition >= 0 && linePosition < lines[lineNumber].Length)
            {
                var line = lines[lineNumber];
                var start = Math.Max(0, linePosition - 20);
                var end = Math.Min(line.Length, linePosition + 20);

                var snippetBefore = line.Substring(start, linePosition - start);
                var snippetAfter = line.Substring(linePosition, end - linePosition);
                var message = ex.Message + " (Json snippet '" + snippetBefore + "<--error-->" + snippetAfter + "')";

                // Not risking updating JSON.net from 9.x to 10.x just to get this as public ctor.
                var ctor = typeof(JsonReaderException).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(Exception), typeof(string), typeof(int), typeof(int) }, null);
                if (ctor != null)
                {
                    return (JsonReaderException)ctor.Invoke(new object[] { message, ex, ex.Path, ex.LineNumber, linePosition });
                }

                // JSON.net 10.x ctor in case we update later.
                ctor = typeof(JsonReaderException).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(string), typeof(int), typeof(int), typeof(Exception) }, null);
                if (ctor != null)
                {
                    return (JsonReaderException)ctor.Invoke(new object[] { message, ex.Path, ex.LineNumber, linePosition, ex });
                }
            }

            return ex;
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
