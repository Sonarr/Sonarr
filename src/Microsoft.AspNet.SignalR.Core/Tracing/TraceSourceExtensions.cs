// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Diagnostics;

namespace System.Diagnostics
{
    public static class TraceSourceExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "msg")]
        public static void TraceVerbose(this TraceSource traceSource, string msg)
        {
            Trace(traceSource, TraceEventType.Verbose, msg);
        }

        public static void TraceVerbose(this TraceSource traceSource, string format, params object[] args)
        {
            Trace(traceSource, TraceEventType.Verbose, format, args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "msg")]
        public static void TraceWarning(this TraceSource traceSource, string msg)
        {
            Trace(traceSource, TraceEventType.Warning, msg);
        }

        public static void TraceWarning(this TraceSource traceSource, string format, params object[] args)
        {
            Trace(traceSource, TraceEventType.Warning, format, args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "msg")]
        public static void TraceError(this TraceSource traceSource, string msg)
        {
            Trace(traceSource, TraceEventType.Error, msg);
        }

        public static void TraceError(this TraceSource traceSource, string format, params object[] args)
        {
            Trace(traceSource, TraceEventType.Error, format, args);
        }

        private static void Trace(TraceSource traceSource, TraceEventType eventType, string msg)
        {
            traceSource.TraceEvent(eventType, 0, msg);
        }

        private static void Trace(TraceSource traceSource, TraceEventType eventType, string format, params object[] args)
        {
            traceSource.TraceEvent(eventType, 0, format, args);
        }
    }
}
