using System.Collections.Generic;
using NzbDrone.Common.Reflection;

namespace NzbDrone.Api.ClientSchema
{
    public static class SchemaBuilder
    {
        public static List<Field> GenerateSchema(object model)
        {
            var properties = model.GetType().GetSimpleProperties();

            var result = new List<Field>(properties.Count);

            foreach (var propertyInfo in properties)
            {
                var fieldAttribute = propertyInfo.GetAttribute<FieldDefinitionAttribute>();

                var field = new Field()
                    {
                        Name = propertyInfo.Name,
                        Label = fieldAttribute.Label,
                        HelpText = fieldAttribute.HelpText,
                        Order = fieldAttribute.Order,

                    };

                var value = propertyInfo.GetValue(model, null);
                if (value != null)
                {
                    field.Value = value.ToString();
                }

                result.Add(field);
            }

            return result;

        }
    }
}