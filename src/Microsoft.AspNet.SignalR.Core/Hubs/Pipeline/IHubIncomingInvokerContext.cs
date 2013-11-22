// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// A description of a server-side hub method invocation originating from a client.
    /// </summary>
    public interface IHubIncomingInvokerContext
    {
        /// <summary>
        /// A hub instance that contains the invoked method as a member.
        /// </summary>
        IHub Hub { get; }

        /// <summary>
        /// A description of the method being invoked by the client.
        /// </summary>
        MethodDescriptor MethodDescriptor { get; }

        /// <summary>
        /// The arguments to be passed to the invoked method.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This represents an ordered list of parameter values")]
        IList<object> Args { get; }

        /// <summary>
        /// A key-value store representing the hub state on the client at the time of the invocation.
        /// </summary>
        StateChangeTracker StateTracker { get; }
    }
}
