// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// Holds information about a single hub method.
    /// </summary>
    public class MethodDescriptor : Descriptor
    {
        /// <summary>
        /// The return type of this method.
        /// </summary>
        public virtual Type ReturnType { get; set; }

        /// <summary>
        /// Hub descriptor object, target to this method.
        /// </summary>
        public virtual HubDescriptor Hub { get; set; }

        /// <summary>
        /// Available method parameters.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is supposed to be mutable")]
        public virtual IList<ParameterDescriptor> Parameters { get; set; }

        /// <summary>
        /// Method invocation delegate.
        /// Takes a target hub and an array of invocation arguments as it's arguments.
        /// </summary>
        public virtual Func<IHub, object[], object> Invoker { get; set; }

        /// <summary>
        /// Attributes attached to this method.
        /// </summary>
        public virtual IEnumerable<Attribute> Attributes { get; set; }
    }
}

