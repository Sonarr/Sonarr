using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Core.Helpers
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
            var split = reader.Value.ToString().Split(':');

            if (split.Count() != 3)
            {
                throw new ArgumentException("TimeSpan is invalid");
            }
            
            return new TimeSpan(int.Parse(split[0]), // hours
                                    int.Parse(split[1]), // minutes
                                    int.Parse(split[2])  // seconds
                                    );
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan);
        }
    }
}
