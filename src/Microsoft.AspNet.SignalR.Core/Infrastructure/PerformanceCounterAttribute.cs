// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    internal sealed class PerformanceCounterAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PerformanceCounterType CounterType { get; set; }
    }
}
