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
        private static readonly JsonSerializerSettings SerializerSetting;
        private static readonly JsonSerializerSettings DeserializerSetting;


        static Json()
        {
            SerializerSetting = new JsonSerializerSettings
                        {
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                            NullValueHandling = NullValueHandling.Ignore,
                            Formatting = Formatting.Indented,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        };


            SerializerSetting.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            SerializerSetting.Converters.Add(new VersionConverter());
            SerializerSetting.Converters.Add(new HttpUriConverter());

            Serializer = JsonSerializer.Create(SerializerSetting);


            DeserializerSetting = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };


            DeserializerSetting.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            DeserializerSetting.Converters.Add(new VersionConverter());
        }

        public static T Deserialize<T>(string json) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(json, DeserializerSetting);
        }

        public static object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, DeserializerSetting);
        }

        public static bool TryDeserialize<T>(string json, out T result) where T : new()
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
            return JsonConvert.SerializeObject(obj, SerializerSetting);
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