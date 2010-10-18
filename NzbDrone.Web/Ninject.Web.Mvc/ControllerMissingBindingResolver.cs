// 
// Authors: Nate Kohari <nate@enkari.com>, Remo Gloor <remo.gloor@gmail.com>
// Copyright (c) 2007-2010, Enkari, Ltd. and contributors
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

namespace Ninject.Web.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Ninject.Activation;
    using Ninject.Activation.Providers;
    using Ninject.Components;
    using Ninject.Infrastructure;
    using Ninject.Parameters;
    using Ninject.Planning.Bindings;
    using Ninject.Planning.Bindings.Resolvers;

    /// <summary>
    /// Missing binding resolver that creates a binding for unknown controllers.
    /// </summary>
    public class ControllerMissingBindingResolver : NinjectComponent, IMissingBindingResolver
    {
        /// <summary>
        /// Returns any bindings from the specified collection that match the specified request.
        /// </summary>
        /// <param name="bindings">The multimap of all registered bindings.</param>
        /// <param name="request">The request in question.</param>
        /// <returns>The series of matching bindings.</returns>
        public IEnumerable<IBinding> Resolve(Multimap<Type, IBinding> bindings, IRequest request)
        {
            var service = request.Service;
            if (typeof(Controller).IsAssignableFrom(service))
            {
                var binding = new Binding(service) { ProviderCallback = StandardProvider.GetCreationCallback(service) };
                binding.Parameters.Add(
                    typeof(AsyncController).IsAssignableFrom(service)
                        ? new PropertyValue("ActionInvoker", ctx => ctx.Kernel.Get<NinjectAsyncActionInvoker>())
                        : new PropertyValue("ActionInvoker", ctx => ctx.Kernel.Get<NinjectActionInvoker>()));
                return new[] { binding };
            }

            return Enumerable.Empty<IBinding>();
        }
    }
}