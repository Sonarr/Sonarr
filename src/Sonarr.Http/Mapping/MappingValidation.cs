using System;
using System.Linq;
using System.Reflection;
using NzbDrone.Common.Reflection;
using Sonarr.Http.REST;

namespace Sonarr.Http.Mapping
{
    public static class MappingValidation
    {
        public static void ValidateMapping(Type modelType, Type resourceType)
        {
            var errors = modelType.GetSimpleProperties().Where(c=>!c.GetGetMethod().IsStatic).Select(p => GetError(resourceType, p)).Where(c => c != null).ToList();

            if (errors.Any())
            {
                throw new ResourceMappingException(errors);
            }

            PrintExtraProperties(modelType, resourceType);
        }

        private static void PrintExtraProperties(Type modelType, Type resourceType)
        {
            var resourceBaseProperties = typeof(RestResource).GetProperties().Select(c => c.Name);
            var resourceProperties = resourceType.GetProperties().Select(c => c.Name).Except(resourceBaseProperties);
            var modelProperties = modelType.GetProperties().Select(c => c.Name);

            var extra = resourceProperties.Except(modelProperties);

            foreach (var extraProp in extra)
            {
                Console.WriteLine("Extra: [{0}]", extraProp);
            }
        }

        private static string GetError(Type resourceType, PropertyInfo modelProperty)
        {
            var resourceProperty = resourceType.GetProperties().FirstOrDefault(c => c.Name == modelProperty.Name);

            if (resourceProperty == null)
            {
                return string.Format("public {0} {1} {{ get; set; }}", modelProperty.PropertyType.Name, modelProperty.Name);
            }

            if (resourceProperty.PropertyType != modelProperty.PropertyType && !typeof(RestResource).IsAssignableFrom(resourceProperty.PropertyType))
            {
                return string.Format("Expected {0}.{1} to have type of {2} but found {3}", resourceType.Name, resourceProperty.Name, modelProperty.PropertyType, resourceProperty.PropertyType);
            }

            return null;
        }
    }
}