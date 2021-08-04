using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace NzbDrone.Core.Datastore.Converters
{
    public class StringListConverter<T> : EmbeddedDocumentConverter<List<string>>
    {
        public override List<string> Parse(object value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            var stringValue = (string)value;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            // Handle when the database contains a comma separated string. Specifically for ReleaseProfiles when ppl downgraded Sonarr versions and added profiles.
            if (!stringValue.StartsWith("[") || !stringValue.EndsWith("]"))
            {
                return stringValue.Split(',').ToList();
            }

            return JsonSerializer.Deserialize<List<string>>((string)value, SerializerSettings);
        }
    }
}
