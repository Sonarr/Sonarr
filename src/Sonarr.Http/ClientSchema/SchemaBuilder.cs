using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Annotations;

namespace Sonarr.Http.ClientSchema
{
    public static class SchemaBuilder
    {
        public static List<Field> ToSchema(object model)
        {
            Ensure.That(model, () => model).IsNotNull();

            var properties = model.GetType().GetSimpleProperties();

            var result = new List<Field>(properties.Count);

            foreach (var propertyInfo in properties)
            {
                var fieldAttribute = propertyInfo.GetAttribute<FieldDefinitionAttribute>(false);

                if (fieldAttribute != null)
                {

                    var field = new Field
                    {
                        Name = propertyInfo.Name,
                        Label = fieldAttribute.Label,
                        HelpText = fieldAttribute.HelpText,
                        HelpLink = fieldAttribute.HelpLink,
                        Order = fieldAttribute.Order,
                        Advanced = fieldAttribute.Advanced,
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

            return result.OrderBy(r => r.Order).ToList();
        }

        public static object ReadFromSchema(List<Field> fields, Type targetType)
        {
            Ensure.That(targetType, () => targetType).IsNotNull();

            var properties = targetType.GetSimpleProperties();

            var target = Activator.CreateInstance(targetType);

            foreach (var propertyInfo in properties)
            {
                var fieldAttribute = propertyInfo.GetAttribute<FieldDefinitionAttribute>(false);

                if (fieldAttribute != null)
                {
                    var field = fields.Find(f => f.Name == propertyInfo.Name);

                    if (propertyInfo.PropertyType == typeof(int))
                    {
                        var value = field.Value.ToString().ParseInt32();
                        propertyInfo.SetValue(target, value ?? 0, null);
                    }

                    else if (propertyInfo.PropertyType == typeof(long))
                    {
                        var value = field.Value.ToString().ParseInt64();
                        propertyInfo.SetValue(target, value ?? 0, null);
                    }

                    else if (propertyInfo.PropertyType == typeof(int?))
                    {
                        var value = field.Value.ToString().ParseInt32();
                        propertyInfo.SetValue(target, value, null);
                    }

                    else if (propertyInfo.PropertyType == typeof(Nullable<Int64>))
                    {
                        var value = field.Value.ToString().ParseInt64();
                        propertyInfo.SetValue(target, value, null);
                    }

                    else if (propertyInfo.PropertyType == typeof(IEnumerable<int>))
                    {
                        IEnumerable<int> value;

                        if (field.Value.GetType() == typeof(JArray))
                        {
                            value = ((JArray)field.Value).Select(s => s.Value<int>());
                        }

                        else
                        {
                            value = field.Value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt32(s));
                        }

                        propertyInfo.SetValue(target, value, null);
                    }

                    else if (propertyInfo.PropertyType == typeof(IEnumerable<string>))
                    {
                        IEnumerable<string> value;

                        if (field.Value.GetType() == typeof(JArray))
                        {
                            value = ((JArray)field.Value).Select(s => s.Value<string>());
                        }

                        else
                        {
                            value = field.Value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        }

                        propertyInfo.SetValue(target, value, null);
                    }

                    else
                    {
                        propertyInfo.SetValue(target, field.Value, null);
                    }
                }
            }

            return target;

        }

        public static T ReadFromSchema<T>(List<Field> fields)
        {
            return (T)ReadFromSchema(fields, typeof(T));
        }

        private static List<SelectOption> GetSelectOptions(Type selectOptions)
        {
            var options = from Enum e in Enum.GetValues(selectOptions)
                          select new SelectOption { Value = Convert.ToInt32(e), Name = e.ToString() };

            return options.OrderBy(o => o.Value).ToList();
        }
    }
}
