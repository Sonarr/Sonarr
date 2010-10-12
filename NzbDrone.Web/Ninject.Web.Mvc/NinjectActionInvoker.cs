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
using System.Linq;
using System.Web.Mvc;
#endregion

namespace Ninject.Web.Mvc
{
	/// <summary>
	/// An <see cref="IActionInvoker"/> that injects filters with dependencies.
	/// </summary>
	public class NinjectActionInvoker : ControllerActionInvoker
	{
		/// <summary>
		/// Gets or sets the kernel.
		/// </summary>
		public IKernel Kernel { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NinjectActionInvoker"/> class.
		/// </summary>
		/// <param name="kernel">The kernel.</param>
		public NinjectActionInvoker(IKernel kernel)
		{
			Kernel = kernel;
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

			foreach (IActionFilter filter in filterInfo.ActionFilters.Where(f => f != null))
				Kernel.Inject(filter);

			foreach (IAuthorizationFilter filter in filterInfo.AuthorizationFilters.Where(f => f != null))
				Kernel.Inject(filter);

			foreach (IExceptionFilter filter in filterInfo.ExceptionFilters.Where(f => f != null))
				Kernel.Inject(filter);

			foreach (IResultFilter filter in filterInfo.ResultFilters.Where(f => f != null))
				Kernel.Inject(filter);

			return filterInfo;
		}
	}
}