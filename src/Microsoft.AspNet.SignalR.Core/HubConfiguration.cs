// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR
{
    public class HubConfiguration : ConnectionConfiguration
    {
        /// <summary>
        /// Determines whether JavaScript proxies for the server-side hubs should be auto generated at {Path}/hubs.
        /// Defaults to true.
        /// </summary>
        public bool EnableJavaScriptProxies { get; set; }

        /// <summary>
        /// Determines whether detailed exceptions thrown in Hub methods get reported back the invoking client.
        /// Defaults to false.
        /// </summary>
        public bool EnableDetailedErrors { get; set; }

        public HubConfiguration()
        {
            EnableJavaScriptProxies = true;
        }
    }
}
