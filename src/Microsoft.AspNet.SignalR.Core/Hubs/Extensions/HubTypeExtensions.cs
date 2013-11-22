// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Hubs
{
    internal static class HubTypeExtensions
    {
        internal static string GetHubName(this Type type)
        {
            if (!typeof(IHub).IsAssignableFrom(type))
            {
                return null;
            }

            return GetHubAttributeName(type) ?? type.Name;
        }

        internal static string GetHubAttributeName(this Type type)
        {
            if (!typeof(IHub).IsAssignableFrom(type))
            {
                return null;
            }

            // We can still return null if there is no attribute name
            return ReflectionHelper.GetAttributeValue<HubNameAttribute, string>(type, attr => attr.HubName);
        }
    }
}
