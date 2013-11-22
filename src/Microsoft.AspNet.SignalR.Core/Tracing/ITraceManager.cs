// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Diagnostics;

namespace Microsoft.AspNet.SignalR.Tracing
{
    public interface ITraceManager
    {
        SourceSwitch Switch { get; }
        TraceSource this[string name] { get; }
    }
}
