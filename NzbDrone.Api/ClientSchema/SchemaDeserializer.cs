using System.Collections.Generic;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Api.ClientSchema
{
    public static class SchemaDeserializer
    {
        public static object DeserializeSchema(object model, List<Field> fields)
        {
            var properties = model.GetType().GetSimpleProperties();

            foreach (var propertyInfo in properties)
            {
                var fieldAttribute = propertyInfo.GetAttribute<FieldDefinitionAttribute>(false);

                if (fieldAttribute != null)
                {
                    var field = fields.Find(f => f.Name == propertyInfo.Name);
                    propertyInfo.SetValue(model, field.Value, null);
                }
            }

            return model;
        }
    }
}