using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.NzbVortex.JsonConverters
{
    public class NzbVortexLoginResultTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var priorityType = (NzbVortexLoginResultType)value;
            writer.WriteValue(priorityType.ToString().ToLower());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = reader.Value.ToString().Replace("_", string.Empty);

            Enum.TryParse(result, true, out NzbVortexLoginResultType output);

            return output;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NzbVortexLoginResultType);
        }
    }
}
