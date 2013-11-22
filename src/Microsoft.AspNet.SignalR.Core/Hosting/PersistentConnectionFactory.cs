// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.AspNet.SignalR.Hosting
{
    /// <summary>
    /// Responsible for creating <see cref="PersistentConnection"/> instances.
    /// </summary>
    public class PersistentConnectionFactory
    {
        private readonly IDependencyResolver _resolver;

        /// <summary>
        /// Creates a new instance of the <see cref="PersistentConnectionFactory"/> class.
        /// </summary>
        /// <param name="resolver">The dependency resolver to use for when creating the <see cref="PersistentConnection"/>.</param>
        public PersistentConnectionFactory(IDependencyResolver resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }

            _resolver = resolver;
        }

        /// <summary>
        /// Creates an instance of the specified type using the dependency resolver or the type's default constructor.
        /// </summary>
        /// <param name="connectionType">The type of <see cref="PersistentConnection"/> to create.</param>
        /// <returns>An instance of a <see cref="PersistentConnection"/>. </returns>
        public PersistentConnection CreateInstance(Type connectionType)
        {
            if (connectionType == null)
            {
                throw new ArgumentNullException("connectionType");
            }

            var connection = (_resolver.Resolve(connectionType) ??
                              Activator.CreateInstance(connectionType)) as PersistentConnection;

            if (connection == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Error_IsNotA, connectionType.FullName, typeof(PersistentConnection).FullName));
            }

            return connection;
        }
    }
}
