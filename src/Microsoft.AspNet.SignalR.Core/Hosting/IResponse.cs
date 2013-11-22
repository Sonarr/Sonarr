// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hosting
{
    /// <summary>
    /// Represents a connection to the client.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Gets a cancellation token that represents the client's lifetime.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets or sets the content type of the response.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Writes buffered data.
        /// </summary>
        /// <param name="data">The data to write to the buffer.</param>
        void Write(ArraySegment<byte> data);

        /// <summary>
        /// Flushes the buffered response to the client.
        /// </summary>
        /// <returns>A task that represents when the data has been flushed.</returns>
        Task Flush();

        /// <summary>
        /// Closes the connection to the client.
        /// </summary>
        /// <returns>A task that represents when the connection is closed.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "End", Justification = "Ends the response thus the name is appropriate.")]
        Task End();
    }
}
