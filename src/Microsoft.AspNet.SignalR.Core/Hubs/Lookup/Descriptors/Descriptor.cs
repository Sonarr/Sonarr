// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Hubs
{
    public abstract class Descriptor
    {
        /// <summary>
        /// Name of Descriptor.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Flags whether the name was specified.
        /// </summary>
        public virtual bool NameSpecified { get; set; }
    }
}
