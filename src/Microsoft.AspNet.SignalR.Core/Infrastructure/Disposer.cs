// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Helper class to manage disposing a resource at an arbirtary time
    /// </summary>
    internal class Disposer : IDisposable
    {
        private static readonly object _disposedSentinel = new object();

        private object _disposable;

        public void Set(IDisposable disposable)
        {
            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            object originalFieldValue = Interlocked.CompareExchange(ref _disposable, disposable, null);
            if (originalFieldValue == null)
            {
                // this is the first call to Set() and Dispose() hasn't yet been called; do nothing
            }
            else if (originalFieldValue == _disposedSentinel)
            {
                // Dispose() has already been called, so we need to dispose of the object that was just added
                disposable.Dispose();
            }
            else
            {
#if !NET35 && !SILVERLIGHT && !NETFX_CORE
                // Set has been called multiple times, fail
                Debug.Fail("Multiple calls to Disposer.Set(IDisposable) without calling Disposer.Dispose()");
#endif
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var disposable = Interlocked.Exchange(ref _disposable, _disposedSentinel) as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
