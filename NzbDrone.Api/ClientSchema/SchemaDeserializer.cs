using System;
using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Api.ClientSchema
{
    public static class SchemaDeserializer
    {
        public static T DeserializeSchema<T>(T model, List<Field> fields)
        {
            var properties = model.GetType().GetSimpleProperties();

            foreach (var propertyInfo in properties)
            {
                var fieldAttribute = propertyInfo.GetAttribute<FieldDefinitionAttribute>(false);

                if (fieldAttribute != null)
                {
                    var field = fields.Find(f => f.Name == propertyInfo.Name);
                    
                    if (propertyInfo.PropertyType == typeof (Int32))
                    {
                        var intValue = Convert.ToInt32(field.Value);
                        propertyInfo.SetValue(model, intValue, null);
                    }

                    else if (propertyInfo.PropertyType == typeof(Nullable<Int32>))
                    {
                        var intValue = field.Value.ToString().ParseInt32();
                        propertyInfo.SetValue(model, intValue, null);
                    }

                    else
                    {
                        propertyInfo.SetValue(model, field.Value, null);
                    }
                }
            }

            return model;
        }
    }
}