// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace Microsoft.AspNet.SignalR
{
    /// <summary>
    /// Provides access to information about a <see cref="IHub"/>.
    /// </summary>
    public interface IHubContext
    {
        /// <summary>
        /// Encapsulates all information about a SignalR connection for an <see cref="IHub"/>.
        /// </summary>
        IHubConnectionContext Clients { get; }

        /// <summary>
        /// Gets the <see cref="IGroupManager"/> the hub.
        /// </summary>
        IGroupManager Groups { get; }
    }
}
