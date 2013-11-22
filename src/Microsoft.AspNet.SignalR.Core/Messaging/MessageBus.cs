// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Configuration;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Tracing;

namespace Microsoft.AspNet.SignalR.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageBus : IMessageBus, IDisposable
    {
        private readonly MessageBroker _broker;

        // The size of the messages store we allocate per topic.
        private readonly uint _messageStoreSize;

        // By default, topics are cleaned up after having no subscribers and after 
        // an interval based on the disconnect timeout has passed. While this works in normal cases
        // it's an issue when the rate of incoming connections is too high. 
        // This is the maximum number of un-expired topics with no subscribers 
        // we'll leave hanging around. The rest will be cleaned up on an the gc interval.
        private readonly int _maxTopicsWithNoSubscriptions;

        private readonly IStringMinifier _stringMinifier;

        private readonly ITraceManager _traceManager;
        private readonly TraceSource _trace;

        private Timer _gcTimer;
        private int _gcRunning;
        private static readonly TimeSpan _gcInterval = TimeSpan.FromSeconds(5);

        private readonly TimeSpan _topicTtl;

        // For unit testing
        internal Action<string, Topic> BeforeTopicGarbageCollected;
        internal Action<string, Topic> AfterTopicGarbageCollected;
        internal Action<string, Topic> BeforeTopicMarked;
        internal Action<string> BeforeTopicCreated;
        internal Action<string, Topic> AfterTopicMarkedSuccessfully;
        internal Action<string, Topic, int> AfterTopicMarked;

        private const int DefaultMaxTopicsWithNoSubscriptions = 1000;

        private readonly Func<string, Topic> _createTopic;
        private readonly Action<ISubscriber, string> _addEvent;
        private readonly Action<ISubscriber, string> _removeEvent;
        private readonly Action<object> _disposeSubscription;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolver"></param>
        public MessageBus(IDependencyResolver resolver)
            : this(resolver.Resolve<IStringMinifier>(),
                   resolver.Resolve<ITraceManager>(),
                   resolver.Resolve<IPerformanceCounterManager>(),
                   resolver.Resolve<IConfigurationManager>(),
                   DefaultMaxTopicsWithNoSubscriptions)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringMinifier"></param>
        /// <param name="traceManager"></param>
        /// <param name="performanceCounterManager"></param>
        /// <param name="configurationManager"></param>
        /// <param name="maxTopicsWithNoSubscriptions"></param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The message broker is disposed when the bus is disposed.")]
        public MessageBus(IStringMinifier stringMinifier,
                          ITraceManager traceManager,
                          IPerformanceCounterManager performanceCounterManager,
                          IConfigurationManager configurationManager,
                          int maxTopicsWithNoSubscriptions)
        {
            if (stringMinifier == null)
            {
                throw new ArgumentNullException("stringMinifier");
            }

            if (traceManager == null)
            {
                throw new ArgumentNullException("traceManager");
            }

            if (performanceCounterManager == null)
            {
                throw new ArgumentNullException("performanceCounterManager");
            }

            if (configurationManager == null)
            {
                throw new ArgumentNullException("configurationManager");
            }

            if (configurationManager.DefaultMessageBufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(Resources.Error_BufferSizeOutOfRange);
            }

            _stringMinifier = stringMinifier;
            _traceManager = traceManager;
            Counters = performanceCounterManager;
            _trace = _traceManager["SignalR." + typeof(MessageBus).Name];
            _maxTopicsWithNoSubscriptions = maxTopicsWithNoSubscriptions;

            _gcTimer = new Timer(_ => GarbageCollectTopics(), state: null, dueTime: _gcInterval, period: _gcInterval);

            _broker = new MessageBroker(Counters)
            {
                Trace = _trace
            };

            // The default message store size
            _messageStoreSize = (uint)configurationManager.DefaultMessageBufferSize;

            _topicTtl = configurationManager.TopicTtl();
            _createTopic = CreateTopic;
            _addEvent = AddEvent;
            _removeEvent = RemoveEvent;
            _disposeSubscription = DisposeSubscription;

            Topics = new TopicLookup();
        }

        protected virtual TraceSource Trace
        {
            get
            {
                return _trace;
            }
        }

        protected internal TopicLookup Topics { get; private set; }
        protected IPerformanceCounterManager Counters { get; private set; }

        public int AllocatedWorkers
        {
            get
            {
                return _broker.AllocatedWorkers;
            }
        }

        public int BusyWorkers
        {
            get
            {
                return _broker.BusyWorkers;
            }
        }

        /// <summary>
        /// Publishes a new message to the specified event on the bus.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        public virtual Task Publish(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Topic topic;
            if (Topics.TryGetValue(message.Key, out topic))
            {
                topic.Store.Add(message);
                ScheduleTopic(topic);
            }

            Counters.MessageBusMessagesPublishedTotal.Increment();
            Counters.MessageBusMessagesPublishedPerSec.Increment();


            return TaskAsyncHelper.Empty;
        }

        protected ulong Save(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            // GetTopic will return a topic for the given key. If topic exists and is Dying, 
            // it will revive it and mark it as NoSubscriptions
            Topic topic = GetTopic(message.Key);
            // Mark the topic as used so it doesn't immediately expire (if it was in that state before).
            topic.MarkUsed();

            return topic.Store.Add(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="cursor"></param>
        /// <param name="callback"></param>
        /// <param name="maxMessages"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The disposable object is returned to the caller")]
        public virtual IDisposable Subscribe(ISubscriber subscriber, string cursor, Func<MessageResult, object, Task<bool>> callback, int maxMessages, object state)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            Subscription subscription = CreateSubscription(subscriber, cursor, callback, maxMessages, state);

            // Set the subscription for this subscriber
            subscriber.Subscription = subscription;

            var topics = new HashSet<Topic>();

            foreach (var key in subscriber.EventKeys)
            {
                // Create or retrieve topic and set it as HasSubscriptions
                Topic topic = SubscribeTopic(key);

                // Set the subscription for this topic
                subscription.SetEventTopic(key, topic);

                topics.Add(topic);
            }

            subscriber.EventKeyAdded += _addEvent;
            subscriber.EventKeyRemoved += _removeEvent;
            subscriber.WriteCursor = subscription.WriteCursor;

            var subscriptionState = new SubscriptionState(subscriber);
            var disposable = new DisposableAction(_disposeSubscription, subscriptionState);

            // When the subscription itself is disposed then dispose it
            subscription.Disposable = disposable;

            // Add the subscription when it's all set and can be scheduled
            // for work. It's important to do this after everything is wired up for the
            // subscription so that publishes can schedule work at the right time.
            foreach (var topic in topics)
            {
                topic.AddSubscription(subscription);
            }

            subscriptionState.Initialized.Set();

            // If there's a cursor then schedule work for this subscription
            if (!String.IsNullOrEmpty(cursor))
            {
                _broker.Schedule(subscription);
            }

            return disposable;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Called from derived class")]
        protected virtual Subscription CreateSubscription(ISubscriber subscriber, string cursor, Func<MessageResult, object, Task<bool>> callback, int messageBufferSize, object state)
        {
            return new DefaultSubscription(subscriber.Identity, subscriber.EventKeys, Topics, cursor, callback, messageBufferSize, _stringMinifier, Counters, state);
        }

        protected void ScheduleEvent(string eventKey)
        {
            Topic topic;
            if (Topics.TryGetValue(eventKey, out topic))
            {
                ScheduleTopic(topic);
            }
        }

        private void ScheduleTopic(Topic topic)
        {
            try
            {
                topic.SubscriptionLock.EnterReadLock();

                for (int i = 0; i < topic.Subscriptions.Count; i++)
                {
                    ISubscription subscription = topic.Subscriptions[i];
                    _broker.Schedule(subscription);
                }
            }
            finally
            {
                topic.SubscriptionLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Creates a topic for the specified key.
        /// </summary>
        /// <param name="key">The key to create the topic for.</param>
        /// <returns>A <see cref="Topic"/> for the specified key.</returns>
        protected virtual Topic CreateTopic(string key)
        {
            // REVIEW: This can be called multiple times, should we guard against it?
            Counters.MessageBusTopicsCurrent.Increment();

            return new Topic(_messageStoreSize, _topicTtl);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Stop the broker from doing any work
                _broker.Dispose();

                // Spin while we wait for the timer to finish if it's currently running
                while (Interlocked.Exchange(ref _gcRunning, 1) == 1)
                {
                    Thread.Sleep(250);
                }

                // Remove all topics
                Topics.Clear();

                if (_gcTimer != null)
                {
                    _gcTimer.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        internal void GarbageCollectTopics()
        {
            if (Interlocked.Exchange(ref _gcRunning, 1) == 1)
            {
                return;
            }

            int topicsWithNoSubs = 0;

            foreach (var pair in Topics)
            {
                if (pair.Value.IsExpired)
                {
                    if (BeforeTopicGarbageCollected != null)
                    {
                        BeforeTopicGarbageCollected(pair.Key, pair.Value);
                    }

                    // Mark the topic as dead
                    DestroyTopic(pair.Key, pair.Value);
                }
                else if (pair.Value.State == TopicState.NoSubscriptions)
                {
                    // Keep track of the number of topics with no subscriptions
                    topicsWithNoSubs++;
                }
            }

            int overflow = topicsWithNoSubs - _maxTopicsWithNoSubscriptions;
            if (overflow > 0)
            {
                // If we've overflowed the max the collect topics that don't have
                // subscribers
                var candidates = new List<KeyValuePair<string, Topic>>();
                foreach (var pair in Topics)
                {
                    if (pair.Value.State == TopicState.NoSubscriptions)
                    {
                        candidates.Add(pair);
                    }
                }

                // We want to remove the overflow but oldest first
                candidates.Sort((leftPair, rightPair) => leftPair.Value.LastUsed.CompareTo(rightPair.Value.LastUsed));

                // Clear up to the overflow and stay within bounds
                for (int i = 0; i < overflow && i < candidates.Count; i++)
                {
                    var pair = candidates[i];
                    // We only want to kill the topic if it's in the NoSubscriptions or Dying state.
                    if (InterlockedHelper.CompareExchangeOr(ref pair.Value.State, TopicState.Dead, TopicState.NoSubscriptions, TopicState.Dying))
                    {
                        // Kill it
                        DestroyTopicCore(pair.Key, pair.Value);
                    }
                }
            }

            Interlocked.Exchange(ref _gcRunning, 0);
        }

        private void DestroyTopic(string key, Topic topic)
        {
            // The goal of this function is to destroy topics after 2 garbage collect cycles
            // This first if statement will transition a topic into the dying state on the first GC cycle 
            // but it will prevent the code path from hitting the second if statement
            if (Interlocked.CompareExchange(ref topic.State, TopicState.Dying, TopicState.NoSubscriptions) == TopicState.Dying)
            {
                // If we've hit this if statement we're on the second GC cycle with this soon to be
                // destroyed topic.  At this point we move the Topic State into the Dead state as
                // long as it has not been revived from the dying state.  We check if the state is
                // still dying again to ensure that the topic has not been transitioned into a new
                // state since we've decided to destroy it.
                if (Interlocked.CompareExchange(ref topic.State, TopicState.Dead, TopicState.Dying) == TopicState.Dying)
                {
                    DestroyTopicCore(key, topic);
                }
            }
        }

        private void DestroyTopicCore(string key, Topic topic)
        {
            Topics.TryRemove(key);
            _stringMinifier.RemoveUnminified(key);

            Counters.MessageBusTopicsCurrent.Decrement();

            Trace.TraceInformation("RemoveTopic(" + key + ")");

            if (AfterTopicGarbageCollected != null)
            {
                AfterTopicGarbageCollected(key, topic);
            }
        }

        internal Topic GetTopic(string key)
        {
            Topic topic;
            int oldState;

            do
            {
                if (BeforeTopicCreated != null)
                {
                    BeforeTopicCreated(key);
                }

                topic = Topics.GetOrAdd(key, _createTopic);

                if (BeforeTopicMarked != null)
                {
                    BeforeTopicMarked(key, topic);
                }

                // If the topic was dying revive it to the NoSubscriptions state.  This is used to ensure
                // that in the scaleout case that even if we're publishing to a topic with no subscriptions
                // that we keep it around in case a user hops nodes.
                oldState = Interlocked.CompareExchange(ref topic.State, TopicState.NoSubscriptions, TopicState.Dying);

                if (AfterTopicMarked != null)
                {
                    AfterTopicMarked(key, topic, topic.State);
                }

                // If the topic is currently dead then we're racing with the DestroyTopicCore function, therefore
                // loop around until we're able to create a new topic
            } while (oldState == TopicState.Dead);

            if (AfterTopicMarkedSuccessfully != null)
            {
                AfterTopicMarkedSuccessfully(key, topic);
            }

            return topic;
        }

        internal Topic SubscribeTopic(string key)
        {
            Topic topic;

            do
            {
                if (BeforeTopicCreated != null)
                {
                    BeforeTopicCreated(key);
                }

                topic = Topics.GetOrAdd(key, _createTopic);

                if (BeforeTopicMarked != null)
                {
                    BeforeTopicMarked(key, topic);
                }

                // Transition into the HasSubscriptions state as long as the topic is not dead
                InterlockedHelper.CompareExchangeOr(ref topic.State, TopicState.HasSubscriptions, TopicState.NoSubscriptions, TopicState.Dying);

                if (AfterTopicMarked != null)
                {
                    AfterTopicMarked(key, topic, topic.State);
                }

                // If we were unable to transition into the HasSubscription state that means we're in the Dead state.
                // Loop around until we're able to create the topic new
            } while (topic.State != TopicState.HasSubscriptions);

            if (AfterTopicMarkedSuccessfully != null)
            {
                AfterTopicMarkedSuccessfully(key, topic);
            }

            return topic;
        }

        private void AddEvent(ISubscriber subscriber, string eventKey)
        {
            Topic topic = SubscribeTopic(eventKey);

            // Add or update the cursor (in case it already exists)
            if (subscriber.Subscription.AddEvent(eventKey, topic))
            {
                // Add it to the list of subs
                topic.AddSubscription(subscriber.Subscription);
            }
        }

        private void RemoveEvent(ISubscriber subscriber, string eventKey)
        {
            Topic topic;
            if (Topics.TryGetValue(eventKey, out topic))
            {
                topic.RemoveSubscription(subscriber.Subscription);
                subscriber.Subscription.RemoveEvent(eventKey);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Failure to invoke the callback should be ignored")]
        private void DisposeSubscription(object state)
        {
            var subscriptionState = (SubscriptionState)state;
            var subscriber = subscriptionState.Subscriber;

            // This will stop work from continuting to happen
            subscriber.Subscription.Dispose();

            try
            {
                // Invoke the terminal callback
                subscriber.Subscription.Invoke(MessageResult.TerminalMessage).Wait();
            }
            catch
            {
                // We failed to talk to the subscriber because they are already gone
                // so the terminal message isn't required.
            }

            subscriptionState.Initialized.Wait();

            subscriber.EventKeyAdded -= _addEvent;
            subscriber.EventKeyRemoved -= _removeEvent;
            subscriber.WriteCursor = null;

            for (int i = subscriber.EventKeys.Count - 1; i >= 0; i--)
            {
                string eventKey = subscriber.EventKeys[i];
                RemoveEvent(subscriber, eventKey);
            }
        }

        private class SubscriptionState
        {
            public ISubscriber Subscriber { get; private set; }
            public ManualResetEventSlim Initialized { get; private set; }

            public SubscriptionState(ISubscriber subscriber)
            {
                Initialized = new ManualResetEventSlim();
                Subscriber = subscriber;
            }
        }
    }
}
