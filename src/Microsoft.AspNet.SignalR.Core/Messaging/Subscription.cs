// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Messaging
{
    public abstract class Subscription : ISubscription, IDisposable
    {
        private readonly Func<MessageResult, object, Task<bool>> _callback;
        private readonly object _callbackState;
        private readonly IPerformanceCounterManager _counters;

        private int _state;
        private int _subscriptionState;

        private bool Alive
        {
            get
            {
                return _subscriptionState != SubscriptionState.Disposed;
            }
        }

        public string Identity { get; private set; }

        public IList<string> EventKeys { get; private set; }

        public int MaxMessages { get; private set; }

        public IDisposable Disposable { get; set; }

        protected Subscription(string identity, IList<string> eventKeys, Func<MessageResult, object, Task<bool>> callback, int maxMessages, IPerformanceCounterManager counters, object state)
        {
            if (String.IsNullOrEmpty(identity))
            {
                throw new ArgumentNullException("identity");
            }

            if (eventKeys == null)
            {
                throw new ArgumentNullException("eventKeys");
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (maxMessages < 0)
            {
                throw new ArgumentOutOfRangeException("maxMessages");
            }

            if (counters == null)
            {
                throw new ArgumentNullException("counters");
            }

            Identity = identity;
            _callback = callback;
            EventKeys = eventKeys;
            MaxMessages = maxMessages;
            _counters = counters;
            _callbackState = state;

            _counters.MessageBusSubscribersTotal.Increment();
            _counters.MessageBusSubscribersCurrent.Increment();
            _counters.MessageBusSubscribersPerSec.Increment();
        }

        public virtual Task<bool> Invoke(MessageResult result)
        {
            return Invoke(result, state => { }, state: null);
        }

        private Task<bool> Invoke(MessageResult result, Action<object> beforeInvoke, object state)
        {
            // Change the state from idle to invoking callback
            var prevState = Interlocked.CompareExchange(ref _subscriptionState,
                                                        SubscriptionState.InvokingCallback,
                                                        SubscriptionState.Idle);

            if (prevState == SubscriptionState.Disposed)
            {
                // Only allow terminal messages after dispose
                if (!result.Terminal)
                {
                    return TaskAsyncHelper.False;
                }
            }

            beforeInvoke(state);

            _counters.MessageBusMessagesReceivedTotal.IncrementBy(result.TotalCount);
            _counters.MessageBusMessagesReceivedPerSec.IncrementBy(result.TotalCount);

            return _callback.Invoke(result, _callbackState).ContinueWith(task =>
            {
                // Go from invoking callback to idle
                Interlocked.CompareExchange(ref _subscriptionState,
                                            SubscriptionState.Idle,
                                            SubscriptionState.InvokingCallback);
                return task;
            },
            TaskContinuationOptions.ExecuteSynchronously).FastUnwrap();
        }

        public Task Work()
        {
            // Set the state to working
            Interlocked.Exchange(ref _state, State.Working);

            var tcs = new TaskCompletionSource<object>();

            WorkImpl(tcs);

            // Fast Path
            if (tcs.Task.IsCompleted)
            {
                return tcs.Task;
            }

            return FinishAsync(tcs);
        }

        public bool SetQueued()
        {
            return Interlocked.Increment(ref _state) == State.Working;
        }

        public bool UnsetQueued()
        {
            // If we try to set the state to idle and we were not already in the working state then keep going
            return Interlocked.CompareExchange(ref _state, State.Idle, State.Working) != State.Working;
        }

        private static Task FinishAsync(TaskCompletionSource<object> tcs)
        {
            return tcs.Task.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return TaskAsyncHelper.FromError(task.Exception);
                }

                return TaskAsyncHelper.Empty;
            }).FastUnwrap();
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "We have a sync and async code path.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to avoid user code taking the process down.")]
        private void WorkImpl(TaskCompletionSource<object> taskCompletionSource)
        {
        Process:
            if (!Alive)
            {
                // If this subscription is dead then return immediately
                taskCompletionSource.TrySetResult(null);
                return;
            }

            var items = new List<ArraySegment<Message>>();
            int totalCount;
            object state;

            PerformWork(items, out totalCount, out state);

            if (items.Count > 0)
            {
                var messageResult = new MessageResult(items, totalCount);
                Task<bool> callbackTask = Invoke(messageResult, s => BeforeInvoke(s), state);

                if (callbackTask.IsCompleted)
                {
                    try
                    {
                        // Make sure exceptions propagate
                        callbackTask.Wait();

                        if (callbackTask.Result)
                        {
                            // Sync path
                            goto Process;
                        }
                        else
                        {
                            // If we're done pumping messages through to this subscription
                            // then dispose
                            Dispose();

                            // If the callback said it's done then stop
                            taskCompletionSource.TrySetResult(null);
                        }
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetUnwrappedException(ex);
                    }
                }
                else
                {
                    WorkImplAsync(callbackTask, taskCompletionSource);
                }
            }
            else
            {
                taskCompletionSource.TrySetResult(null);
            }
        }

        protected virtual void BeforeInvoke(object state)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "The list needs to be populated")]
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "The caller wouldn't be able to specify what the generic type argument is")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "The count needs to be returned")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "The state needs to be set by the callee")]
        protected abstract void PerformWork(IList<ArraySegment<Message>> items, out int totalCount, out object state);

        private void WorkImplAsync(Task<bool> callbackTask, TaskCompletionSource<object> taskCompletionSource)
        {
            // Async path
            callbackTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    taskCompletionSource.TrySetUnwrappedException(task.Exception);
                }
                else if (task.Result)
                {
                    WorkImpl(taskCompletionSource);
                }
                else
                {
                    // If we're done pumping messages through to this subscription
                    // then dispose
                    Dispose();

                    // If the callback said it's done then stop
                    taskCompletionSource.TrySetResult(null);
                }
            });
        }

        public virtual bool AddEvent(string key, Topic topic)
        {
            return AddEventCore(key);
        }

        public virtual void RemoveEvent(string key)
        {
            lock (EventKeys)
            {
                EventKeys.Remove(key);
            }
        }

        public virtual void SetEventTopic(string key, Topic topic)
        {
            // Don't call AddEvent since that's virtual
            AddEventCore(key);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // REIVIEW: Consider sleeping instead of using a tight loop, or maybe timing out after some interval
                // if the client is very slow then this invoke call might not end quickly and this will make the CPU
                // hot waiting for the task to return.

                var spinWait = new SpinWait();

                while (true)
                {
                    // Wait until the subscription isn't working anymore
                    var state = Interlocked.CompareExchange(ref _subscriptionState,
                                                            SubscriptionState.Disposed,
                                                            SubscriptionState.Idle);

                    // If we're not working then stop
                    if (state != SubscriptionState.InvokingCallback)
                    {
                        if (state != SubscriptionState.Disposed)
                        {
                            // Only decrement if we're not disposed already
                            _counters.MessageBusSubscribersCurrent.Decrement();
                            _counters.MessageBusSubscribersPerSec.Decrement();
                        }

                        // Raise the disposed callback
                        if (Disposable != null)
                        {
                            Disposable.Dispose();
                        }

                        break;
                    }

                    spinWait.SpinOnce();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public abstract void WriteCursor(TextWriter textWriter);

        private bool AddEventCore(string key)
        {
            lock (EventKeys)
            {
                if (EventKeys.Contains(key))
                {
                    return false;
                }

                EventKeys.Add(key);
                return true;
            }
        }

        private static class State
        {
            public const int Idle = 0;
            public const int Working = 1;
        }

        private static class SubscriptionState
        {
            public const int Idle = 0;
            public const int InvokingCallback = 1;
            public const int Disposed = 2;
        }
    }
}
