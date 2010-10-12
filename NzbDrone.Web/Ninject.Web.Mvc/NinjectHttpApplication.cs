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
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject.Infrastructure;
#endregion

namespace Ninject.Web.Mvc
{
	/// <summary>
	/// Defines an <see cref="HttpApplication"/> that is controlled by a Ninject <see cref="IKernel"/>.
	/// </summary>
	public abstract class NinjectHttpApplication : HttpApplication, IHaveKernel
	{
		private static IKernel _kernel;

		/// <summary>
		/// Gets the kernel.
		/// </summary>
		public IKernel Kernel
		{
			get { return _kernel; }
		}

		/// <summary>
		/// Starts the application.
		/// </summary>
		public void Application_Start()
		{
			lock (this)
			{
				_kernel = CreateKernel();

				_kernel.Bind<RouteCollection>().ToConstant(RouteTable.Routes);
				_kernel.Bind<HttpContext>().ToMethod(ctx => HttpContext.Current).InTransientScope();
                _kernel.Bind<HttpContextBase>().ToMethod(ctx => new HttpContextWrapper(HttpContext.Current)).InTransientScope();
				
				ControllerBuilder.Current.SetControllerFactory(CreateControllerFactory());

				_kernel.Inject(this);

				OnApplicationStarted();
			}
		}

		/// <summary>
		/// Stops the application.
		/// </summary>
		public void Application_Stop()
		{
			lock (this)
			{
				if (_kernel != null)
				{
					_kernel.Dispose();
					_kernel = null;
				}

				OnApplicationStopped();
			}
		}

		/// <summary>
		/// Creates the kernel that will manage your application.
		/// </summary>
		/// <returns>The created kernel.</returns>
		protected abstract IKernel CreateKernel();

		/// <summary>
		/// Creates the controller factory that is used to create the controllers.
		/// </summary>
		/// <returns>The created controller factory.</returns>
		protected virtual NinjectControllerFactory CreateControllerFactory()
		{
			return new NinjectControllerFactory(Kernel);
		}

		/// <summary>
		/// Called when the application is started.
		/// </summary>
		protected virtual void OnApplicationStarted() { }

		/// <summary>
		/// Called when the application is stopped.
		/// </summary>
		protected virtual void OnApplicationStopped() { }
	}
}