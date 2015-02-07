// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    internal static class CancellationTokenExtensions
    {
        private delegate CancellationTokenRegistration RegisterDelegate(ref CancellationToken token, Action<object> callback, object state);

        private static readonly RegisterDelegate _tokenRegister = ResolveRegisterDelegate();

        public static IDisposable SafeRegister(this CancellationToken cancellationToken, Action<object> callback, object state)
        {
            var callbackWrapper = new CancellationCallbackWrapper(callback, state);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            CancellationTokenRegistration registration = _tokenRegister(ref cancellationToken, s => InvokeCallback(s), callbackWrapper);

            var disposeCancellationState = new DiposeCancellationState(callbackWrapper, registration);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return new DisposableAction(s => Dispose(s), disposeCancellationState);
        }

        private static void InvokeCallback(object state)
        {
            ((CancellationCallbackWrapper)state).TryInvoke();
        }

        private static void Dispose(object state)
        {
            ((DiposeCancellationState)state).TryDispose();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method should never throw since it runs as part of field initialzation")]
        private static RegisterDelegate ResolveRegisterDelegate()
        {
            // The fallback is just a normal register that capatures the execution context.
            RegisterDelegate fallback = (ref CancellationToken token, Action<object> callback, object state) =>
            {
                return token.Register(callback, state);
            };

#if NETFX_CORE || PORTABLE
            return fallback;
#else

            MethodInfo methodInfo = null;

            try
            {
                // By default we don't want to capture the execution context,
                // since this is internal we need to create a delegate to this up front
                methodInfo = typeof(CancellationToken).GetMethod("InternalRegisterWithoutEC",
                                                                 BindingFlags.NonPublic | BindingFlags.Instance,
                                                                 binder: null,
                                                                 types: new[] { typeof(Action<object>), typeof(object) },
                                                                 modifiers: null);
            }
            catch
            {
                // Swallow this exception. Being extra paranoid, we don't want anything to break in case this dirty
                // reflection hack fails for any reason
            }

            // If the method was removed then fallback to the regular method
            if (methodInfo == null)
            {
                return fallback;
            }

            try
            {

                return (RegisterDelegate)Delegate.CreateDelegate(typeof(RegisterDelegate), null, methodInfo);
            }
            catch
            {
                // If this fails for whatever reason just fallback to normal register
                return fallback;
            }
#endif
        }

        private class DiposeCancellationState
        {
            private readonly CancellationCallbackWrapper _callbackWrapper;
            private readonly CancellationTokenRegistration _registration;

            public DiposeCancellationState(CancellationCallbackWrapper callbackWrapper, CancellationTokenRegistration registration)
            {
                _callbackWrapper = callbackWrapper;
                _registration = registration;
            }

            public void TryDispose()
            {
                // This normally waits until the callback is finished invoked but we don't care
                if (_callbackWrapper.TrySetInvoked())
                {
                    try
                    {
                        _registration.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Bug #1549, .NET 4.0 has a bug where this throws if the CTS is disposed.
                    }
                }
            }
        }

        private class CancellationCallbackWrapper
        {
            private readonly Action<object> _callback;
            private readonly object _state;
            private int _callbackInvoked;

            public CancellationCallbackWrapper(Action<object> callback, object state)
            {
                _callback = callback;
                _state = state;
            }

            public bool TrySetInvoked()
            {
                return Interlocked.Exchange(ref _callbackInvoked, 1) == 0;
            }

            public void TryInvoke()
            {
                if (TrySetInvoked())
                {
                    _callback(_state);
                }
            }
        }
    }
}
