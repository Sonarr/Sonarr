// 
// Authors: Nate Kohari <nate@enkari.com>, Remo Gloor <remo.gloor@gmail.com>
// Copyright (c) 2007-2010, Enkari, Ltd. and contributors
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

namespace Ninject.Web.Mvc
{
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// Injects all filters of a filter info.
    /// </summary>
    public class FilterInjector : IFilterInjector
    {
        /// <summary>
        /// The kernel
        /// </summary>
        private readonly IKernel kernel;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterInjector"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public FilterInjector(IKernel kernel)
        {
            this.kernel = kernel;
        }

        /// <summary>
        /// Injects all filters of the specified filter info.
        /// </summary>
        /// <param name="filterInfo">The filter info.</param>
        public void Inject(FilterInfo filterInfo)
        {
            foreach (IActionFilter filter in filterInfo.ActionFilters.Where(f => f != null))
            {
                this.kernel.Inject(filter);
            }

            foreach (IAuthorizationFilter filter in filterInfo.AuthorizationFilters.Where(f => f != null))
            {
                this.kernel.Inject(filter);
            }

            foreach (IExceptionFilter filter in filterInfo.ExceptionFilters.Where(f => f != null))
            {
                this.kernel.Inject(filter);
            }

            foreach (IResultFilter filter in filterInfo.ResultFilters.Where(f => f != null))
            {
                this.kernel.Inject(filter);
            }            
        }
    }
}