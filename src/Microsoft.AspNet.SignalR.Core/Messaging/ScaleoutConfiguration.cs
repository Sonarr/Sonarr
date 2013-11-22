// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Messaging
{
    /// <summary>
    /// Common settings for scale-out message bus implementations.
    /// </summary>
    public class ScaleoutConfiguration
    {
        public static readonly int DisableQueuing = 0;

        private int _maxQueueLength;

        /// <summary>
        /// The maximum length of the outgoing send queue. Messages being sent to the backplane are queued
        /// up to this length. After the max length is reached, further sends will throw an <see cref="System.InvalidOperationException">InvalidOperationException</see>.
        /// Set to <see cref="Microsoft.AspNet.SignalR.Messaging.ScaleoutConfiguration.DisableQueuing">ScaleoutConfiguration.DisableQueuing</see> to disable queing.
        /// Defaults to disabled.
        /// </summary>
        public virtual int MaxQueueLength
        {
            get
            {
                return _maxQueueLength;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _maxQueueLength = value;
            }
        }
    }
}
