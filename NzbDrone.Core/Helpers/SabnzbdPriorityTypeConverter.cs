using System;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.Model.Sabnzbd;

namespace NzbDrone.Core.Helpers
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
