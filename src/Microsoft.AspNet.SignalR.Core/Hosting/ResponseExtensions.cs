// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hosting
{
    /// <summary>
    /// Extension methods for <see cref="IResponse"/>.
    /// </summary>
    public static class ResponseExtensions
    {
        /// <summary>
        /// Closes the connection to a client with optional data.
        /// </summary>
        /// <param name="response">The <see cref="IResponse"/>.</param>
        /// <param name="data">The data to write to the connection.</param>
        /// <returns>A task that represents when the connection is closed.</returns>
        public static Task End(this IResponse response, string data)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var bytes = Encoding.UTF8.GetBytes(data);
            response.Write(new ArraySegment<byte>(bytes, 0, bytes.Length));
            return response.End();
        }
    }
}
