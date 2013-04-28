using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NzbDrone.Common.Reflection
{
    public static class ReflectionExtensions
    {
        public static List<PropertyInfo> GetSimpleProperties(this Type type)
        {
            var properties = type.GetProperties();
            return properties.Where(c => c.PropertyType.IsSimpleType()).ToList();
        }


        public static List<Type> ImplementationsOf<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(c => typeof(T).IsAssignableFrom(c)).ToList();
        }


        public static bool IsSimpleType(this Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>) || type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                type = type.GetGenericArguments()[0];
            }


            return type.IsPrimitive
                   || type.IsEnum
                   || type == typeof(string)
                   || type == typeof(DateTime)
                   || type == typeof(Decimal);
        }

        public static bool IsReadable(this PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead && propertyInfo.GetGetMethod(false) != null;
        }

        public static bool IsWritable(this PropertyInfo propertyInfo)
        {
            return propertyInfo.CanWrite && propertyInfo.GetSetMethod(false) != null;
        }

    }
}