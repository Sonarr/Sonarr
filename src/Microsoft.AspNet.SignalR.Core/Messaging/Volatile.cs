// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Messaging
{
    // All methods here are guaranteed both volatile +  atomic.
    // TODO: Make this use the .NET 4.5 'Volatile' type.
    internal static class Volatile
    {
        public static long Read(ref long location)
        {
            return Interlocked.Read(ref location);
        }
    }
}
