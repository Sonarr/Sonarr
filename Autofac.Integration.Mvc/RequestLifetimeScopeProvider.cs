// This software is part of the Autofac IoC container
// Copyright (c) 2011 Autofac Contributors
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Creates and disposes HTTP request based lifetime scopes.
    /// </summary>
    /// <remarks>
    /// The provider is notified when a HTTP request ends by the <see cref="RequestLifetimeHttpModule"/>.
    /// </remarks>
    public class RequestLifetimeScopeProvider : ILifetimeScopeProvider
    {
        readonly ILifetimeScope _container;

        /// <summary>
        /// Tag used to identify registrations that are scoped to the HTTP request level.
        /// </summary>
        internal static readonly object HttpRequestTag = "AutofacWebRequest";

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLifetimeScopeProvider"/> class.
        /// </summary>
        /// <param name="container">The parent container, usually the application container.</param>
        public RequestLifetimeScopeProvider(ILifetimeScope container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
            RequestLifetimeHttpModule.SetLifetimeScopeProvider(this);
        }

        /// <summary>
        /// Gets the global, application-wide container.
        /// </summary>
        public ILifetimeScope ApplicationContainer
        {
            get { return _container; }
        }

        static ILifetimeScope LifetimeScope
        {
            get { return (ILifetimeScope)HttpContext.Current.Items[typeof(ILifetimeScope)]; }
            set { HttpContext.Current.Items[typeof(ILifetimeScope)] = value; }
        }

        /// <summary>
        /// Gets a nested lifetime scope that services can be resolved from.
        /// </summary>
        /// <param name="configurationAction">
        /// A configuration action that will execute during lifetime scope creation.
        /// </param>
        /// <returns>A new or existing nested lifetime scope.</returns>
        public ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(RequestLifetimeScopeProviderResources.HttpContextNotAvailable);
            }

            if (LifetimeScope == null)
            {
                if ((LifetimeScope = GetLifetimeScopeCore(configurationAction)) == null)
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, RequestLifetimeScopeProviderResources.NullLifetimeScopeReturned, GetType().FullName));
            }
            return LifetimeScope;
        }

        /// <summary>
        /// Ends the current HTTP request lifetime scope.
        /// </summary>
        public void EndLifetimeScope()
        {
            var lifetimeScope = LifetimeScope;
            if (lifetimeScope != null)
                lifetimeScope.Dispose();
        }

        /// <summary>
        /// Gets a lifetime scope for the current HTTP request. This method can be overridden
        /// to alter the way that the life time scope is constructed.
        /// </summary>
        /// <param name="configurationAction">
        /// A configuration action that will execute during lifetime scope creation.
        /// </param>
        /// <returns>A new lifetime scope for the current HTTP request.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual ILifetimeScope GetLifetimeScopeCore(Action<ContainerBuilder> configurationAction)
        {
            return (configurationAction == null)
                       ? ApplicationContainer.BeginLifetimeScope(HttpRequestTag)
                       : ApplicationContainer.BeginLifetimeScope(HttpRequestTag, configurationAction);
        }
    }
}
