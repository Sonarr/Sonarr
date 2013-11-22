// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// Interface to be implemented by <see cref="System.Attribute"/>s that can authorize client to connect to a <see cref="IHub"/>.
    /// </summary>
    public interface IAuthorizeHubConnection
    {
        /// <summary>
        /// Given a <see cref="HubCallerContext"/>, determine whether client is authorized to connect to <see cref="IHub"/>.
        /// </summary>
        /// <param name="hubDescriptor">Description of the hub client is attempting to connect to.</param>
        /// <param name="request">The connection request from the client.</param>
        /// <returns>true if the caller is authorized to connect to the hub; otherwise, false.</returns>
        bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request);
    }
}
