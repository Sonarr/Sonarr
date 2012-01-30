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
            if (value is TimeSpan)
                writer.WriteValue(value.ToString());

            else
                throw new Exception("Expected TimeSpan object value.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var split = reader.Value.ToString().Split(':');

            if (split.Count() == 3)
            {
                return new TimeSpan(int.Parse(split[0]), // hours
                                    int.Parse(split[1]), // minutes
                                    int.Parse(split[2])  // seconds
                                    );
            }
            
            throw new ArgumentException("TimeSpan is invalid");
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(TimeSpan))
                return true;

            return false;
        }
    }
}
