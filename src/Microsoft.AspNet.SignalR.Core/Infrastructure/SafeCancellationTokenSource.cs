// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Thread safe cancellation token source. Allows the following:
    /// - Cancel will no-op if the token is disposed.
    /// - Dispose may be called after Cancel.
    /// </summary>
    internal class SafeCancellationTokenSource : IDisposable
    {
        private CancellationTokenSource _cts;
        private int _state;

        public SafeCancellationTokenSource()
        {
            _cts = new CancellationTokenSource();
            Token = _cts.Token;
        }

        public CancellationToken Token { get; private set; }

        public void Cancel()
        {
            var value = Interlocked.CompareExchange(ref _state, State.Cancelling, State.Initial);

            if (value == State.Initial)
            {
                // Because cancellation tokens are so poorly behaved, always invoke the cancellation token on 
                // another thread. Don't capture any of the context (execution context or sync context)
                // while doing this.
#if WINDOWS_PHONE || SILVERLIGHT
                ThreadPool.QueueUserWorkItem(_ =>
#elif NETFX_CORE
                Task.Run(() =>
#else
                ThreadPool.UnsafeQueueUserWorkItem(_ =>
#endif
                {
                    try
                    {
                        _cts.Cancel();
                    }
                    finally
                    {
                        if (Interlocked.CompareExchange(ref _state, State.Cancelled, State.Cancelling) == State.Disposing)
                        {
                            _cts.Dispose();
                            Interlocked.Exchange(ref _state, State.Disposed);
                        }
                    }
                }
#if !NETFX_CORE
                , state: null
#endif
);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var value = Interlocked.Exchange(ref _state, State.Disposing);

                switch (value)
                {
                    case State.Initial:
                    case State.Cancelled:
                        _cts.Dispose();
                        Interlocked.Exchange(ref _state, State.Disposed);
                        break;
                    case State.Cancelling:
                    case State.Disposing:
                        // No-op
                        break;
                    case State.Disposed:
                        Interlocked.Exchange(ref _state, State.Disposed);
                        break;
                    default:
                        break;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private static class State
        {
            public const int Initial = 0;
            public const int Cancelling = 1;
            public const int Cancelled = 2;
            public const int Disposing = 3;
            public const int Disposed = 4;
        }
    }
}
