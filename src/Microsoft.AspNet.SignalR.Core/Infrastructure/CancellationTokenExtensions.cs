// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    internal static class CancellationTokenExtensions
    {
        public static IDisposable SafeRegister(this CancellationToken cancellationToken, Action<object> callback, object state)
        {
            var callbackWrapper = new CancellationCallbackWrapper(callback, state);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            CancellationTokenRegistration registration = cancellationToken.Register(s => Cancel(s),
                                                                                    callbackWrapper,
                                                                                    useSynchronizationContext: false);

            var disposeCancellationState = new DiposeCancellationState(callbackWrapper, registration);

            // Ensure delegate continues to use the C# Compiler static delegate caching optimization.
            return new DisposableAction(s => Dispose(s), disposeCancellationState);
        }

        private static void Cancel(object state)
        {
            ((CancellationCallbackWrapper)state).TryInvoke();
        }

        private static void Dispose(object state)
        {
            ((DiposeCancellationState)state).TryDispose();
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
