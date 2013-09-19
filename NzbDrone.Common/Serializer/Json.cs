using System;
using System.IO;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace NzbDrone.Common.Serializer
{
    public static class Json
    {
        private static readonly JsonSerializer Serializer;
        private static readonly JsonSerializerSettings SerializerSetting;

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
            SerializerSetting.Converters.Add(new IntConverter());
            SerializerSetting.Converters.Add(new VersionConverter());

            Serializer = JsonSerializer.Create(SerializerSetting);

        }

        public static T Deserialize<T>(string json) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(json, SerializerSetting);
        }

        public static object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, SerializerSetting);
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