using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Converters
{
    public class QualityIntConverter : JsonConverter<Quality>
    {
        public override Quality Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item = reader.GetInt32();
            return (Quality)item;
        }

        public override void Write(Utf8JsonWriter writer, Quality value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }

    public class DapperQualityIntConverter : SqlMapper.TypeHandler<Quality>
    {
        public override void SetValue(IDbDataParameter parameter, Quality value)
        {
            parameter.Value = value == null ? 0 : (int)value;
        }

        public override Quality Parse(object value)
        {
            return (Quality)Convert.ToInt32(value);
        }
    }
}
