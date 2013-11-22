// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Owin
{
    internal static class OwinEnvironmentExtensions
    {
        internal static T Get<T>(this IDictionary<string, object> environment, string key)
        {
            object value;
            return environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        internal static CancellationToken GetShutdownToken(this IDictionary<string, object> env)
        {
            object value;
            return env.TryGetValue(OwinConstants.HostOnAppDisposing, out value)
                && value is CancellationToken
                ? (CancellationToken)value
                : default(CancellationToken);
        }

        internal static string GetAppInstanceName(this IDictionary<string, object> environment)
        {
            object value;
            if (environment.TryGetValue(OwinConstants.HostAppNameKey, out value))
            {
                var stringVal = value as string;

                if (!String.IsNullOrEmpty(stringVal))
                {
                    return stringVal;
                }
            }

            return null;
        }

        internal static bool SupportsWebSockets(this IDictionary<string, object> environment)
        {
            object value;
            if (environment.TryGetValue(OwinConstants.ServerCapabilities, out value))
            {
                var capabilities = value as IDictionary<string, object>;
                if (capabilities != null)
                {
                    return capabilities.ContainsKey(OwinConstants.WebSocketVersion);
                }
            }
            return false;
        }

        internal static bool GetIsDebugEnabled(this IDictionary<string, object> environment)
        {
            object value;
            if (environment.TryGetValue(OwinConstants.HostAppModeKey, out value))
            {
                var stringVal = value as string;
                return !String.IsNullOrWhiteSpace(stringVal) &&
                       OwinConstants.AppModeDevelopment.Equals(stringVal, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
