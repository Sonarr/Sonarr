// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// A description of a client-side hub method invocation originating from the server.
    /// </summary>
    public interface IHubOutgoingInvokerContext
    {
        /// <summary>
        /// The <see cref="IConnection"/>, if any, corresponding to the client that invoked the server-side hub method
        /// that is invoking the client-side hub method.
        /// </summary>
        IConnection Connection { get; }

        /// <summary>
        /// A description of the method call to be made on the client.
        /// </summary>
        ClientHubInvocation Invocation { get; }

        /// <summary>
        /// The signal (ConnectionId, hub type name or hub type name + "." + group name) belonging to clients that
        /// receive the method invocation.
        /// </summary>
        string Signal { get; }

        /// <summary>
        /// The signals (ConnectionId, hub type name or hub type name + "." + group name) belonging to clients that should
        /// not receive the method invocation regardless of the <see cref="IHubOutgoingInvokerContext.Signal"/>. 
        /// </summary>
        IList<string> ExcludedSignals { get; }
    }
}
