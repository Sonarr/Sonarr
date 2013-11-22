// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR
{
    public class ConnectionConfiguration
    {
        // Resolver isn't set to GlobalHost.DependencyResolver in the ctor because it is lazily created.
        private IDependencyResolver _resolver;

        /// <summary>
        /// The dependency resolver to use for the hub connection.
        /// </summary>
        public IDependencyResolver Resolver
        {
            get { return _resolver ?? GlobalHost.DependencyResolver; }
            set { _resolver = value; }
        }

        /// <summary>
        /// Determines if browsers can make cross domain requests to SignalR endpoints.
        /// </summary>
        public bool EnableCrossDomain { get; set; }
    }
}
