// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR
{
    /// <summary>
    /// A message sent to one more connections.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Messags are never compared, just used as data.")]
    public struct ConnectionMessage
    {
        /// <summary>
        /// The signal to this message should be sent to. Connections subscribed to this signal
        /// will receive the message payload.
        /// </summary>
        public string Signal { get; private set; }

        /// <summary>
        /// The payload of the message.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Represents a list of signals that should be used to filter what connections
        /// receive this message.
        /// </summary>
        public IList<string> ExcludedSignals { get; private set; }

        public ConnectionMessage(string signal, object value)
            : this(signal, value, ListHelper<string>.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionMessage"/> class.
        /// </summary>
        /// <param name="signal">The signal</param>
        /// <param name="value">The payload of the message</param>
        /// <param name="excludedSignals">The signals to exclude.</param>
        public ConnectionMessage(string signal, object value, IList<string> excludedSignals)
            : this()
        {
            Signal = signal;
            Value = value;
            ExcludedSignals = excludedSignals;
        }
    }
}
