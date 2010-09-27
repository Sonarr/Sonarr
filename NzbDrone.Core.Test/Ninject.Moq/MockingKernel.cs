using System;
using Ninject.Activation.Caching;
using Ninject.Planning.Bindings;

namespace Ninject.Moq
{
	/// <summary>
	/// A kernel that will create mocked instances (via Moq) for any service that is
	/// requested for which no binding is registered.
	/// </summary>
	public class MockingKernel : StandardKernel
	{
		/// <summary>
		/// Clears the kernel's cache, immediately deactivating all activated instances regardless of scope.
		/// This does not remove any modules, extensions, or bindings.
		/// </summary>
		public void Reset()
		{
			Components.Get<ICache>().Clear();
		}

		/// <summary>
		/// Attempts to handle a missing binding for a service.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <returns><c>True</c> if the missing binding can be handled; otherwise <c>false</c>.</returns>
		protected override bool HandleMissingBinding(Type service)
		{
			var binding = new Binding(service)
			{
				ProviderCallback = MockProvider.GetCreationCallback(),
				ScopeCallback = ctx => null,
				IsImplicit = true
			};

			AddBinding(binding);

			return true;
		}
	}
}
