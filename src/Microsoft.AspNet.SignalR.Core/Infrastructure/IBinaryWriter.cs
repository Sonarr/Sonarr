// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Implemented on anything that has the ability to write raw binary data
    /// </summary>
    public interface IBinaryWriter
    {
        void Write(ArraySegment<byte> data);
    }
}
