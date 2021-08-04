using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Datastore.Converters
{
    public class DapperLanguageIntConverter : SqlMapper.TypeHandler<Language>
    {
        public override void SetValue(IDbDataParameter parameter, Language value)
        {
            if (value == null)
            {
                throw new InvalidOperationException("Attempted to save a language that isn't really a language");
            }
            else
            {
                parameter.Value = (int)value;
            }
        }

        public override Language Parse(object value)
        {
            if (value == null || value is DBNull)
            {
                return Language.Unknown;
            }

            return (Language)Convert.ToInt32(value);
        }
    }

    public class LanguageIntConverter : JsonConverter<Language>
    {
        public override Language Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var item = reader.GetInt32();
            return (Language)item;
        }

        public override void Write(Utf8JsonWriter writer, Language value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }
}
