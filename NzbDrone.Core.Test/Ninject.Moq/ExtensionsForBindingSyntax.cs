using System;
using Ninject.Infrastructure;
using Ninject.Injection;
using Ninject.Planning.Bindings;
using Ninject.Syntax;

namespace Ninject.Moq
{
	/// <summary>
	/// Extensions for the fluent binding syntax API.
	/// </summary>
	public static class ExtensionsForBindingSyntax
	{
		/// <summary>
		/// Indicates that the service should be bound to a mocked instance of the specified type.
		/// </summary>
		/// <typeparam name="T">The service that is being mocked.</typeparam>
		/// <param name="builder">The builder that is building the binding.</param>
		public static IBindingWhenInNamedWithOrOnSyntax<T> ToMock<T>(this IBindingToSyntax<T> builder)
		{
			var haveBinding = builder as IHaveBinding;
			
			if (haveBinding == null)
				throw new NotSupportedException(String.Format("The binding builder for {0} is of type {1}, which does not implement IHaveBinding and is therefore not extensible.", typeof(T), builder.GetType()));

			IBinding binding = haveBinding.Binding;

			binding.ProviderCallback = ctx => new MockProvider(ctx.Kernel.Components.Get<IInjectorFactory>());

			return builder as IBindingWhenInNamedWithOrOnSyntax<T>;
		}
	}
}
