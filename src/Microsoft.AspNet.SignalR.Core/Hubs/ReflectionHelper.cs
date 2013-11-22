// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.SignalR.Hubs
{
    public static class ReflectionHelper
    {
        private static readonly Type[] _excludeTypes = new[] { typeof(Hub), typeof(object) };
        private static readonly Type[] _excludeInterfaces = new[] { typeof(IHub), typeof(IDisposable) };

        public static IEnumerable<MethodInfo> GetExportedHubMethods(Type type)
        {
            if (!typeof(IHub).IsAssignableFrom(type))
            {
                return Enumerable.Empty<MethodInfo>();
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var allInterfaceMethods = _excludeInterfaces.SelectMany(i => GetInterfaceMethods(type, i));

            return methods.Except(allInterfaceMethods).Where(IsValidHubMethod);

        }

        private static bool IsValidHubMethod(MethodInfo methodInfo)
        {
            return !(_excludeTypes.Contains(methodInfo.GetBaseDefinition().DeclaringType) ||
                     methodInfo.IsSpecialName);
        }

        private static IEnumerable<MethodInfo> GetInterfaceMethods(Type type, Type iface)
        {
            if (!iface.IsAssignableFrom(type))
            {
                return Enumerable.Empty<MethodInfo>();
            }

            return type.GetInterfaceMap(iface).TargetMethods;
        }

        public static TResult GetAttributeValue<TAttribute, TResult>(ICustomAttributeProvider source, Func<TAttribute, TResult> valueGetter)
            where TAttribute : Attribute
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (valueGetter == null)
            {
                throw new ArgumentNullException("valueGetter");
            }

            var attributes = source.GetCustomAttributes(typeof(TAttribute), false)
                .Cast<TAttribute>()
                .ToList();
            if (attributes.Any())
            {
                return valueGetter(attributes[0]);
            }
            return default(TResult);
        }

    }
}
