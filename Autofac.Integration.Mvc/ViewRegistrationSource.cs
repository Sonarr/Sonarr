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
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// A registration source for building view registrations.
    /// </summary>
    /// <remarks>
    /// Supports view registrations for <see cref="WebViewPage"/>, <see cref="ViewPage"/>, 
    /// <see cref="ViewMasterPage"/> and <see cref="ViewUserControl"/> derived types.
    /// </remarks>
    public class ViewRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var typedService = service as IServiceWithType;

            if (typedService != null && IsSupportedView(typedService.ServiceType))
                yield return RegistrationBuilder.ForType(typedService.ServiceType)
                    .PropertiesAutowired()
                    .InstancePerDependency()
                    .CreateRegistration();
        }

        /// <summary>
        /// Gets whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (I.e. like Meta, Func or Owned.)
        /// </summary>
        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }

        static bool IsSupportedView(Type serviceType)
        {
            return serviceType.IsAssignableTo<WebViewPage>()
                || serviceType.IsAssignableTo<ViewPage>()
                || serviceType.IsAssignableTo<ViewMasterPage>()
                || serviceType.IsAssignableTo<ViewUserControl>();
        }
    }
}
