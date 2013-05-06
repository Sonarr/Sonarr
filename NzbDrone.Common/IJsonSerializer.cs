using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NzbDrone.Common
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(string json) where T : class, new();
        string Serialize(object obj);
        void Serialize<TModel>(TModel model, TextWriter textWriter);
        void Serialize<TModel>(TModel model, Stream outputStream);
        object Deserialize(string json, Type type);
    }

    public class JsonSerializer : IJsonSerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _jsonNetSerializer;

        public JsonSerializer()
        {
            var setting = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
                };

            _jsonNetSerializer = new Newtonsoft.Json.JsonSerializer()
                {
                    DateTimeZoneHandling = setting.DateTimeZoneHandling,
                    NullValueHandling = setting.NullValueHandling,
                    Formatting = setting.Formatting,
                    DefaultValueHandling = setting.DefaultValueHandling,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
        }



        public T Deserialize<T>(string json) where T : class, new()
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }


        public void Serialize<TModel>(TModel model, TextWriter outputStream)
        {
            var jsonTextWriter = new JsonTextWriter(outputStream);
            _jsonNetSerializer.Serialize(jsonTextWriter, model);
            jsonTextWriter.Flush();
        }

        public void Serialize<TModel>(TModel model, Stream outputStream)
        {
            Serialize(model, new StreamWriter(outputStream));
        }


    }
}