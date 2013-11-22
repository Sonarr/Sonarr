// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    internal static class PrefixHelper
    {
        // Hubs
        internal const string HubPrefix = "h-";
        internal const string HubGroupPrefix = "hg-";
        internal const string HubConnectionIdPrefix = "hc-";

        // Persistent Connections
        internal const string PersistentConnectionPrefix = "pc-";
        internal const string PersistentConnectionGroupPrefix = "pcg-";

        // Both
        internal const string ConnectionIdPrefix = "c-";
        internal const string AckPrefix = "ack-";

        public static bool HasGroupPrefix(string value)
        {
            return value.StartsWith(HubGroupPrefix, StringComparison.Ordinal) ||
                   value.StartsWith(PersistentConnectionGroupPrefix, StringComparison.Ordinal);
        }

        public static string GetConnectionId(string connectionId)
        {
            return ConnectionIdPrefix + connectionId;
        }

        public static string GetHubConnectionId(string connectionId)
        {
            return HubConnectionIdPrefix + connectionId;
        }

        public static string GetHubName(string connectionId)
        {
            return HubPrefix + connectionId;
        }

        public static string GetHubGroupName(string groupName)
        {
            return HubGroupPrefix + groupName;
        }

        public static string GetPersistentConnectionGroupName(string groupName)
        {
            return PersistentConnectionGroupPrefix + groupName;
        }

        public static string GetPersistentConnectionName(string connectionName)
        {
            return PersistentConnectionPrefix + connectionName;
        }

        public static string GetAck(string connectionId)
        {
            return AckPrefix + connectionId;
        }

        public static IList<string> GetPrefixedConnectionIds(IList<string> connectionIds)
        {
            if (connectionIds.Count == 0)
            {
                return ListHelper<string>.Empty;
            }

            return connectionIds.Select(PrefixHelper.GetConnectionId).ToList();
        }

        public static IEnumerable<string> RemoveGroupPrefixes(IEnumerable<string> groups)
        {
            return groups.Select(PrefixHelper.RemoveGroupPrefix);
        }

        public static string RemoveGroupPrefix(string name)
        {
            if (name.StartsWith(HubGroupPrefix, StringComparison.Ordinal))
            {
                return name.Substring(HubGroupPrefix.Length);
            }

            if (name.StartsWith(PersistentConnectionGroupPrefix, StringComparison.Ordinal))
            {
                return name.Substring(PersistentConnectionGroupPrefix.Length);
            }

            return name;
        }
    }
}
