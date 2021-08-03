using System;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd.JsonConverters
{
    public class SabnzbdQueueTimeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ts = (TimeSpan)value;
            writer.WriteValue(ts.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var split = reader.Value.ToString().Split(':').Select(int.Parse).ToArray();

            switch (split.Count())
            {
                case 4:
                    return new TimeSpan((split[0] * 24) + split[1], split[2], split[3]);
                case 3:
                    return new TimeSpan(split[0], split[1], split[2]);
                default:
                    throw new ArgumentException("Expected either 0:0:0:0 or 0:0:0 format, but received: " + reader.Value);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan);
        }
    }
}
