// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.AspNet.SignalR.Messaging
{
    public class Topic
    {
        private readonly TimeSpan _lifespan;

        // Keeps track of the last time this subscription was used
        private DateTime _lastUsed = DateTime.UtcNow;

        public IList<ISubscription> Subscriptions { get; private set; }
        public MessageStore<Message> Store { get; private set; }
        public ReaderWriterLockSlim SubscriptionLock { get; private set; }

        // State of the topic
        internal int State;

        public virtual bool IsExpired
        {
            get
            {
                try
                {
                    SubscriptionLock.EnterReadLock();

                    TimeSpan timeSpan = DateTime.UtcNow - _lastUsed;

                    return Subscriptions.Count == 0 && timeSpan > _lifespan;
                }
                finally
                {
                    SubscriptionLock.ExitReadLock();
                }
            }
        }

        public DateTime LastUsed
        {
            get
            {
                return _lastUsed;
            }
        }

        public Topic(uint storeSize, TimeSpan lifespan)
        {
            _lifespan = lifespan;
            Subscriptions = new List<ISubscription>();
            Store = new MessageStore<Message>(storeSize);
            SubscriptionLock = new ReaderWriterLockSlim();
        }

        public void MarkUsed()
        {
            this._lastUsed = DateTime.UtcNow;
        }

        public void AddSubscription(ISubscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            try
            {
                SubscriptionLock.EnterWriteLock();

                MarkUsed();

                Subscriptions.Add(subscription);
                
                // Created -> HasSubscriptions
                Interlocked.CompareExchange(ref State,
                                            TopicState.HasSubscriptions,
                                            TopicState.NoSubscriptions);
            }
            finally
            {
                SubscriptionLock.ExitWriteLock();
            }
        }

        public void RemoveSubscription(ISubscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            try
            {
                SubscriptionLock.EnterWriteLock();

                MarkUsed();
              
                Subscriptions.Remove(subscription);
               

                if (Subscriptions.Count == 0)
                {
                    // HasSubscriptions -> NoSubscriptions
                    Interlocked.CompareExchange(ref State,
                                                TopicState.NoSubscriptions,
                                                TopicState.HasSubscriptions);
                }
            }
            finally
            {
                SubscriptionLock.ExitWriteLock();
            }
        }
    }
}
