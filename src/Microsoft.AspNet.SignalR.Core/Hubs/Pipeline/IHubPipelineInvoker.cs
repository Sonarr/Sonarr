// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// Implementations of this interface are responsible for executing operation required to complete various stages
    /// hub processing such as connecting, reconnecting, disconnecting, invoking server-side hub methods, invoking
    /// client-side hub methods, authorizing hub clients and rejoining hub groups.
    /// </summary>
    public interface IHubPipelineInvoker
    {
        /// <summary>
        /// Invokes a server-side hub method.
        /// </summary>
        /// <param name="context">A description of the server-side hub method invocation.</param>
        /// <returns>An asynchronous operation giving the return value of the server-side hub method invocation.</returns>
        Task<object> Invoke(IHubIncomingInvokerContext context);

        /// <summary>
        /// Invokes a client-side hub method.
        /// </summary>
        /// <param name="context">A description of the client-side hub method invocation.</param>
        Task Send(IHubOutgoingInvokerContext context);

        /// <summary>
        /// To be called when a client connects to the <see cref="HubDispatcher"/> for each <see cref="IHub"/> the client
        /// connects to. By default, this results in the <see cref="IHub"/>'s OnConnected method being invoked.
        /// </summary>
        /// <param name="hub">A <see cref="IHub"/> the client is connected to.</param>
        Task Connect(IHub hub);

        /// <summary>
        /// To be called when a client reconnects to the <see cref="HubDispatcher"/> for each <see cref="IHub"/> the client
        /// connects to. By default, this results in the <see cref="IHub"/>'s OnReconnected method being invoked.
        /// </summary>
        /// <param name="hub">A <see cref="IHub"/> the client is reconnected to.</param>
        Task Reconnect(IHub hub);

        /// <summary>
        /// To be called when a client disconnects from the <see cref="HubDispatcher"/> for each <see cref="IHub"/> the client
        /// was connected to. By default, this results in the <see cref="IHub"/>'s OnDisconnected method being invoked.
        /// </summary>
        /// <param name="hub">A <see cref="IHub"/> the client was disconnected from.</param>
        Task Disconnect(IHub hub);

        /// <summary>
        /// To be called before a client subscribes to signals belonging to the hub described by the <see cref="HubDescriptor"/>.
        /// By default, the <see cref="AuthorizeModule"/> will look for attributes on the <see cref="IHub"/> to help determine if
        /// the client is authorized to subscribe to method invocations for the described hub.
        /// </summary>
        /// <param name="hubDescriptor">A description of the hub the client is attempting to connect to.</param>
        /// <param name="request">
        /// The connect request being made by the client which should include the client's
        /// <see cref="System.Security.Principal.IPrincipal"/> User.
        /// </param>
        /// <returns>true, if the client is authorized to subscribe to client-side hub method invocations; false, otherwise.</returns>
        bool AuthorizeConnect(HubDescriptor hubDescriptor, IRequest request);

        /// <summary>
        /// This method determines which of the groups belonging to the hub described by the <see cref="HubDescriptor"/> the client should be
        /// allowed to rejoin.
        /// By default, clients that are reconnecting to the server will be removed from all groups they may have previously been a member of,
        /// because untrusted clients may claim to be a member of groups they were never authorized to join.
        /// </summary>
        /// <param name="hubDescriptor">A description of the hub for which the client is attempting to rejoin groups.</param>
        /// <param name="request">The reconnect request being made by the client that is attempting to rejoin groups.</param>
        /// <param name="groups">
        /// The list of groups belonging to the relevant hub that the client claims to have been a member of before the reconnect.
        /// </param>
        /// <returns>A list of groups the client is allowed to rejoin.</returns>
        IList<string> RejoiningGroups(HubDescriptor hubDescriptor, IRequest request, IList<string> groups);
    }
}
