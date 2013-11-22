// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.SignalR.Messaging
{
    // Represents the result of a call to MessageStore<T>.GetMessages.
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "This is never compared")]
    public struct MessageStoreResult<T> where T : class
    {
        // The first message ID in the result set. Messages in the result set have sequentually increasing IDs.
        // If FirstMessageId = 20 and Messages.Length = 4, then the messages have IDs { 20, 21, 22, 23 }.
        private readonly ulong _firstMessageId;

        // If this is true, the backing MessageStore contains more messages, and the client should call GetMessages again.
        private readonly bool _hasMoreData;

        // The actual result set. May be empty.
        private readonly ArraySegment<T> _messages;

        public MessageStoreResult(ulong firstMessageId, ArraySegment<T> messages, bool hasMoreData)
        {
            _firstMessageId = firstMessageId;
            _messages = messages;
            _hasMoreData = hasMoreData;
        }

        public ulong FirstMessageId
        {
            get
            {
                return _firstMessageId;
            }
        }

        public bool HasMoreData
        {
            get
            {
                return _hasMoreData;
            }
        }

        public ArraySegment<T> Messages
        {
            get
            {
                return _messages;
            }
        }
    }
}
