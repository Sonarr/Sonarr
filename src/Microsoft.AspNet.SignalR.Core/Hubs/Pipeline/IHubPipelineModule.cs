// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// An <see cref="IHubPipelineModule"/> can intercept and customize various stages of hub processing such as connecting,
    /// reconnecting, disconnecting, invoking server-side hub methods, invoking client-side hub methods, authorizing hub
    /// clients and rejoining hub groups.
    /// Modules can be be activated by calling <see cref="IHubPipeline.AddModule"/>.
    /// The combined modules added to the <see cref="IHubPipeline" /> are invoked via the <see cref="IHubPipelineInvoker"/>
    /// interface.
    /// </summary>
    public interface IHubPipelineModule
    {
        /// <summary>
        /// Wraps a function that invokes a server-side hub method. Even if a client has not been authorized to connect
        /// to a hub, it will still be authorized to invoke server-side methods on that hub unless it is prevented in
        /// <see cref="IHubPipelineModule.BuildIncoming"/> by not executing the invoke parameter.
        /// </summary>
        /// <param name="invoke">A function that invokes a server-side hub method.</param>
        /// <returns>A wrapped function that invokes a server-side hub method.</returns>
        Func<IHubIncomingInvokerContext, Task<object>> BuildIncoming(Func<IHubIncomingInvokerContext, Task<object>> invoke);

        /// <summary>
        /// Wraps a function that invokes a client-side hub method.
        /// </summary>
        /// <param name="send">A function that invokes a client-side hub method.</param>
        /// <returns>A wrapped function that invokes a client-side hub method.</returns>
        Func<IHubOutgoingInvokerContext, Task> BuildOutgoing(Func<IHubOutgoingInvokerContext, Task> send);

        /// <summary>
        /// Wraps a function that is called when a client connects to the <see cref="HubDispatcher"/> for each
        /// <see cref="IHub"/> the client connects to. By default, this results in the <see cref="IHub"/>'s
        /// OnConnected method being invoked.
        /// </summary>
        /// <param name="connect">A function to be called when a client connects to a hub.</param>
        /// <returns>A wrapped function to be called when a client connects to a hub.</returns>
        Func<IHub, Task> BuildConnect(Func<IHub, Task> connect);

        /// <summary>
        /// Wraps a function that is called when a client reconnects to the <see cref="HubDispatcher"/> for each
        /// <see cref="IHub"/> the client connects to. By default, this results in the <see cref="IHub"/>'s
        /// OnReconnected method being invoked.
        /// </summary>
        /// <param name="reconnect">A function to be called when a client reconnects to a hub.</param>
        /// <returns>A wrapped function to be called when a client reconnects to a hub.</returns>
        Func<IHub, Task> BuildReconnect(Func<IHub, Task> reconnect);

        /// <summary>
        /// Wraps a function that is called  when a client disconnects from the <see cref="HubDispatcher"/> for each
        /// <see cref="IHub"/> the client was connected to. By default, this results in the <see cref="IHub"/>'s
        /// OnDisconnected method being invoked.
        /// </summary>
        /// <param name="disconnect">A function to be called when a client disconnects from a hub.</param>
        /// <returns>A wrapped function to be called when a client disconnects from a hub.</returns>
        Func<IHub, Task> BuildDisconnect(Func<IHub, Task> disconnect);
        
        /// <summary>
        /// Wraps a function to be called before a client subscribes to signals belonging to the hub described by the
        /// <see cref="HubDescriptor"/>. By default, the <see cref="AuthorizeModule"/> will look for attributes on the
        /// <see cref="IHub"/> to help determine if the client is authorized to subscribe to method invocations for the
        /// described hub.
        /// The function returns true if the client is authorized to subscribe to client-side hub method
        /// invocations; false, otherwise.
        /// </summary>
        /// <param name="authorizeConnect">
        /// A function that dictates whether or not the client is authorized to connect to the described Hub.
        /// </param>
        /// <returns>
        /// A wrapped function that dictates whether or not the client is authorized to connect to the described Hub.
        /// </returns>
        Func<HubDescriptor, IRequest, bool> BuildAuthorizeConnect(Func<HubDescriptor, IRequest, bool> authorizeConnect);
        
        /// <summary>
        /// Wraps a function that determines which of the groups belonging to the hub described by the <see cref="HubDescriptor"/>
        /// the client should be allowed to rejoin.
        /// By default, clients will rejoin all the groups they were in prior to reconnecting.
        /// </summary>
        /// <param name="rejoiningGroups">A function that determines which groups the client should be allowed to rejoin.</param>
        /// <returns>A wrapped function that determines which groups the client should be allowed to rejoin.</returns>
        Func<HubDescriptor, IRequest, IList<string>, IList<string>> BuildRejoiningGroups(Func<HubDescriptor, IRequest, IList<string>, IList<string>> rejoiningGroups);
    }
}
