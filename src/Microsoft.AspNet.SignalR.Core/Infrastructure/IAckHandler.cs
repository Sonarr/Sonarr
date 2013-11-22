// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    public interface IAckHandler
    {
        Task CreateAck(string id);

        bool TriggerAck(string id);
    }
}
