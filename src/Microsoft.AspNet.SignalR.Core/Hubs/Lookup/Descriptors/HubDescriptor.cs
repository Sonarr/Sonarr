// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// Holds information about a single hub.
    /// </summary>
    public class HubDescriptor : Descriptor
    {
        /// <summary>
        /// Hub type.
        /// </summary>
        public virtual Type HubType { get; set; }

        public string CreateQualifiedName(string unqualifiedName)
        {
            return Name + "." + unqualifiedName;
        }
    }
}
