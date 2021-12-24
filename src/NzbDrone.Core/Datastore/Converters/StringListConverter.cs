using Marr.Data.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Linq;
using System;

namespace NzbDrone.Core.Datastore.Converters
{
    public class StringListConverter : EmbeddedDocumentConverter
    {
        public override object FromDB(ConverterContext context)
        {
            if (context.DbValue == DBNull.Value)
            {
                return DBNull.Value;
            }

            var stringValue = (string)context.DbValue;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            // Handle when the database contains a comma separated string. Specifically for ReleaseProfiles when ppl downgraded Sonarr versions and added profiles.
            if (!stringValue.StartsWith("[") || !stringValue.EndsWith("]"))
            {
                return stringValue.Split(',').ToList();
            }
            
            return JsonConvert.DeserializeObject(stringValue, context.ColumnMap.FieldType, SerializerSetting);
        }
    }
}
