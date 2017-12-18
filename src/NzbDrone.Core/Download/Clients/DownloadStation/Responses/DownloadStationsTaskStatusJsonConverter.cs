using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    public class DownloadStationsTaskStatusJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DownloadStationTaskStatus);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = reader.Value.ToString().Replace("_", string.Empty);

            DownloadStationTaskStatus output;

            Enum.TryParse<DownloadStationTaskStatus>(enumString, true, out output);

            return output;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
