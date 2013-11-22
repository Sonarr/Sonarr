// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.SignalR.Messaging
{
    public class Command
    {
        public Command()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public bool WaitForAck { get; set; }
        public string Id { get; private set; }
        public CommandType CommandType { get; set; }
        public string Value { get; set; }
    }
}
