using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd.JsonConverters
{
    public class SabnzbdPriorityTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var priorityType = (SabPriorityType)value;
                writer.WriteValue(priorityType.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var queuePriority = reader.Value.ToString();

            SabPriorityType output;
            Enum.TryParse(queuePriority, out output);

            return output;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SabPriorityType);
        }
    }
}
