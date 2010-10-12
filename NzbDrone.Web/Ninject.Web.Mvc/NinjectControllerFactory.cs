#region License
// 
// Authors: Nate Kohari <nate@enkari.com>, Josh Close <narshe@gmail.com>
// Copyright (c) 2007-2009, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 
#endregion
#region Using Directives
using System;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;
#endregion

namespace Ninject.Web.Mvc
{
    /// <summary>
    /// A controller factory that creates <see cref="IController"/>s via Ninject.
    /// </summary>
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        /// <summary>
        /// Gets the kernel that will be used to create controllers.
        /// </summary>
        public IKernel Kernel { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjectControllerFactory"/> class.
        /// </summary>
        /// <param name="kernel">The kernel that should be used to create controllers.</param>
        public NinjectControllerFactory(IKernel kernel)
        {
            Kernel = kernel;
        }

        /// <summary>
        /// Gets a controller instance of type controllerType.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <param name="controllerType">Type of controller to create.</param>
        /// <returns>The controller instance.</returns>
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                // let the base handle 404 errors with proper culture information
                return base.GetControllerInstance(requestContext, controllerType);
            }

            var controller = Kernel.GetService(controllerType) as IController;

            if (controller == null)
                return base.GetControllerInstance(requestContext, controllerType);

            var standardController = controller as Controller;

            if (standardController != null)
                standardController.ActionInvoker = CreateActionInvoker();

            return controller;
        }

        /// <summary>
        /// Creates the action invoker.
        /// </summary>
        /// <returns>The action invoker.</returns>
        protected virtual NinjectActionInvoker CreateActionInvoker()
        {
            return new NinjectActionInvoker(Kernel);
        }
    }
}