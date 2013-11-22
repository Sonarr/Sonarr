// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.SignalR.Messaging
{
    public class LocalEventKeyInfo
    {
        private readonly WeakReference _storeReference;

        public LocalEventKeyInfo(string key, ulong id, MessageStore<Message> store)
        {
            // Don't hold onto MessageStores that would otherwise be GC'd
            _storeReference = new WeakReference(store);
            Key = key;
            Id = id;
        }

        public string Key { get; private set; }
        public ulong Id { get; private set; }
        public MessageStore<Message> MessageStore
        {
            get
            {
                return _storeReference.Target as MessageStore<Message>;
            }
        }
    }
}
