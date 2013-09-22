using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Api.ClientSchema
{
    public static class SchemaBuilder
    {
        public static List<Field> ToSchema(object model)
        {
            Ensure.That(() => model).IsNotNull();

            var properties = model.GetType().GetSimpleProperties();

            var result = new List<Field>(properties.Count);

            foreach (var propertyInfo in properties)
            {
                var fieldAttribute = propertyInfo.GetAttribute<FieldDefinitionAttribute>(false);

                if (fieldAttribute != null)
                {

                    var field = new Field()
                        {
                            Name = propertyInfo.Name,
                            Label = fieldAttribute.Label,
                            HelpText = fieldAttribute.HelpText,
                            HelpLink = fieldAttribute.HelpLink,
                            Order = fieldAttribute.Order,
                            Type = fieldAttribute.Type.ToString().ToLowerInvariant()
                        };

                    var value = propertyInfo.GetValue(model, null);
                    if (value != null)
                    {
                        field.Value = value;
                    }

                    if (fieldAttribute.Type == FieldType.Select)
                    {
                        field.SelectOptions = GetSelectOptions(fieldAttribute.SelectOptions);
                    }

                    result.Add(field);
                }
            }

            return result;

        }


        public static object ReadFormSchema(List<Field> fields, Type targetType)
        {
            var properties = targetType.GetSimpleProperties();

            var target = Activator.CreateInstance(targetType);

            foreach (var propertyInfo in properties)
            {
                var fieldAttribute = propertyInfo.GetAttribute<FieldDefinitionAttribute>(false);

                if (fieldAttribute != null)
                {
                    var field = fields.Find(f => f.Name == propertyInfo.Name);

                    if (propertyInfo.PropertyType == typeof(Int32))
                    {
                        var intValue = Convert.ToInt32(field.Value);
                        propertyInfo.SetValue(target, intValue, null);
                    }

                    else if (propertyInfo.PropertyType == typeof(Nullable<Int32>))
                    {
                        var intValue = field.Value.ToString().ParseInt32();
                        propertyInfo.SetValue(target, intValue, null);
                    }

                    else
                    {
                        propertyInfo.SetValue(target, field.Value, null);
                    }
                }
            }

            return target;

        }

        public static T ReadFormSchema<T>(List<Field> fields)
        {
            return (T)ReadFormSchema(fields, typeof (T));
        }

        private static List<SelectOption> GetSelectOptions(Type selectOptions)
        {
            var options = from Enum e in Enum.GetValues(selectOptions)
                          select new SelectOption { Value = Convert.ToInt32(e), Name = e.ToString() };

            return options.OrderBy(o => o.Value).ToList();
        }
    }
}