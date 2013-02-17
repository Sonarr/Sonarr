// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Autofac implementation of the <see cref="IDependencyResolver"/> interface.
    /// </summary>
    public class AutofacDependencyResolver : IDependencyResolver
    {
        readonly ILifetimeScope _container;
        readonly Action<ContainerBuilder> _configurationAction;
        ILifetimeScopeProvider _lifetimeScopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container that nested lifetime scopes will be create from.</param>
        public AutofacDependencyResolver(ILifetimeScope container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container that nested lifetime scopes will be create from.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in nested lifetime scopes.</param>
        public AutofacDependencyResolver(ILifetimeScope container, Action<ContainerBuilder> configurationAction)
            : this(container)
        {
            if (configurationAction == null) throw new ArgumentNullException("configurationAction");
            _configurationAction = configurationAction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container that nested lifetime scopes will be create from.</param>
        /// <param name="lifetimeScopeProvider">A <see cref="ILifetimeScopeProvider"/> implementation for 
        /// creating new lifetime scopes.</param>
        public AutofacDependencyResolver(ILifetimeScope container, ILifetimeScopeProvider lifetimeScopeProvider) :
            this(container)
        {
            if (lifetimeScopeProvider == null) throw new ArgumentNullException("lifetimeScopeProvider");
            _lifetimeScopeProvider = lifetimeScopeProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">The container that nested lifetime scopes will be create from.</param>
        /// <param name="lifetimeScopeProvider">A <see cref="ILifetimeScopeProvider"/> implementation for 
        /// creating new lifetime scopes.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registations visible only in nested lifetime scopes.</param>
        public AutofacDependencyResolver(ILifetimeScope container, ILifetimeScopeProvider lifetimeScopeProvider, Action<ContainerBuilder> configurationAction)
            : this(container, lifetimeScopeProvider)
        {
            if (configurationAction == null) throw new ArgumentNullException("configurationAction");
            _configurationAction = configurationAction;
        }

        /// <summary>
        /// Gets the Autofac implementation of the dependency resolver.
        /// </summary>
        public static AutofacDependencyResolver Current
        {
            get
            {
                // Issue 351: We can't necessarily cast the current dependency resolver
                // to AutofacDependencyResolver because diagnostic systems like Glimpse
                // will wrap/proxy the resolver. Instead we need to register the resolver
                // on the fly with the request lifetime scope and resolve it accordingly.
                return DependencyResolver.Current.GetService<AutofacDependencyResolver>();
            }
        }

        /// <summary>
        /// The lifetime containing components for processing the current HTTP request.
        /// </summary>
        public ILifetimeScope RequestLifetimeScope
        {
            get
            {
                // Issue 351: Register the AutofacDependencyResolver with
                // the request lifetime scope so the current resolver can
                // be retrieved without having to cast it directly to
                // this specific type.
                Action<ContainerBuilder> composite = builder =>
                {
                    if (this._configurationAction != null)
                    {
                        this._configurationAction(builder);
                    }
                    builder.RegisterInstance(this).As<AutofacDependencyResolver>();
                };
                if (_lifetimeScopeProvider == null)
                {
                    _lifetimeScopeProvider = new RequestLifetimeScopeProvider(_container);
                }
                return _lifetimeScopeProvider.GetLifetimeScope(composite);
            }
        }

        /// <summary>
        /// Gets the application container that was provided to the constructor.
        /// </summary>
        public ILifetimeScope ApplicationContainer
        {
            get { return _container; }
        }

        /// <summary>
        /// Get a single instance of a service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>The single instance if resolved; otherwise, <c>null</c>.</returns>
        public object GetService(Type serviceType)
        {
            return RequestLifetimeScope.ResolveOptional(serviceType);
        }

        /// <summary>
        /// Gets all available instances of a services.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>The list of instances if any were resolved; otherwise, an empty list.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instance = RequestLifetimeScope.Resolve(enumerableServiceType);
            return (IEnumerable<object>)instance;
        }
    }
}
