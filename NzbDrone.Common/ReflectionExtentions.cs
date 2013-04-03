using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NzbDrone.Common
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetInterfaces(this Assembly assembly)
        {
            return assembly.GetTypes().Where(c => c.IsInterface);
        }

        public static IEnumerable<Type> GetImplementations(this Assembly assembly, Type contractType)
        {
            return assembly.GetTypes()
                .Where(implementation =>
                    contractType.IsAssignableFrom(implementation) &&
                    !implementation.IsInterface &&
                    !implementation.IsAbstract
                );
        }
    }
}