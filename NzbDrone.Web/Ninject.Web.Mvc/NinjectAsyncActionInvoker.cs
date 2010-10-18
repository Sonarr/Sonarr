// 
// Authors: Nate Kohari <nate@enkari.com>, Josh Close <narshe@gmail.com>
// Copyright (c) 2007-2009, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

namespace Ninject.Web.Mvc
{
    using System.Web.Mvc;
    using System.Web.Mvc.Async;

    /// <summary>
    /// An <see cref="IActionInvoker"/> that injects filters with dependencies.
    /// </summary>
    public class NinjectAsyncActionInvoker : AsyncControllerActionInvoker
    {
        private readonly IFilterInjector filterInjector;

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjectAsyncActionInvoker"/> class.
        /// </summary>
        /// <param name="filterInjector">The filter injector.</param>
        public NinjectAsyncActionInvoker(IFilterInjector filterInjector)
        {
            this.filterInjector = filterInjector;
        }

        /// <summary>
        /// Gets the filters for the specified request and action.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>The filters.</returns>
        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            FilterInfo filterInfo = base.GetFilters(controllerContext, actionDescriptor);
            this.filterInjector.Inject(filterInfo);
            return filterInfo;
        }
    }
}