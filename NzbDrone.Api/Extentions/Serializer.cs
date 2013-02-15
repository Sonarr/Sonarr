using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NzbDrone.Api.Extentions
{
    public static class Serializer
    {
        static Serializer()
        {
            Settings = new JsonSerializerSettings
                {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented,
                        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
                };

            Instance = new JsonSerializer
                {
                        DateTimeZoneHandling = Settings.DateTimeZoneHandling,
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented,
                        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                        ContractResolver =  new CamelCasePropertyNamesContractResolver()
                };
        }

        public static JsonSerializerSettings Settings { get; private set; }

        public static JsonSerializer Instance { get; private set; }
    }
}