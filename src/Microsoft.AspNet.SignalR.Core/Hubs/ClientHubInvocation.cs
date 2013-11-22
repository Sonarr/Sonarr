// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// A description of a client-side hub method invocation.
    /// </summary>
    public class ClientHubInvocation
    {
        /// <summary>
        /// The signal that clients receiving this invocation are subscribed to.
        /// </summary>
        [JsonIgnore]
        public string Target { get; set; }

        /// <summary>
        /// The name of the hub that the method being invoked belongs to.
        /// </summary>
        [JsonProperty("H")]
        public string Hub { get; set; }

        /// <summary>
        /// The name of the client-side hub method be invoked.
        /// </summary>
        [JsonProperty("M")]
        public string Method { get; set; }

        /// <summary>
        /// The argument list the client-side hub method will be called with.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Type is used for serialization.")]
        [JsonProperty("A")]
        public object[] Args { get; set; }

        /// <summary>
        /// A key-value store representing the hub state on the server that has changed since the last time the hub
        /// state was sent to the client.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Type is used for serialization.")]
        [JsonProperty("S", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> State { get; set; }
    }
}
