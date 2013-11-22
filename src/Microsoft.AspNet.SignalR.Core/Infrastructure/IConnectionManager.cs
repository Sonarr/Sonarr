// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.SignalR.Hubs;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Provides access to hubs and persistent connections references.
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Returns a <see cref="IHubContext"/> for the specified <see cref="IHub"/>.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IHub"/></typeparam>
        /// <returns>a <see cref="IHubContext"/> for the specified <see cref="IHub"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The hub type needs to be specified")]
        IHubContext GetHubContext<T>() where T : IHub;

        /// <summary>
        /// Returns a <see cref="IHubContext"/>for the specified hub.
        /// </summary>
        /// <param name="hubName">Name of the hub</param>
        /// <returns>a <see cref="IHubContext"/> for the specified hub</returns>
        IHubContext GetHubContext(string hubName);

        /// <summary>
        /// Returns a <see cref="IPersistentConnectionContext"/> for the <see cref="PersistentConnection"/>.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="PersistentConnection"/></typeparam>
        /// <returns>A <see cref="IPersistentConnectionContext"/> for the <see cref="PersistentConnection"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The connection type needs to be specified")]
        IPersistentConnectionContext GetConnectionContext<T>() where T : PersistentConnection;
    }
}
