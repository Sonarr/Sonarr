// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Microsoft.AspNet.SignalR.Messaging
{
    /// <summary>
    /// This class is the main coordinator. It schedules work to be done for a particular subscription 
    /// and has an algorithm for choosing a number of workers (thread pool threads), to handle
    /// the scheduled work.
    /// </summary>
    public class MessageBroker : IDisposable
    {
        private readonly Queue<ISubscription> _queue = new Queue<ISubscription>();

        private readonly IPerformanceCounterManager _counters;

        // The maximum number of workers (threads) allowed to process all incoming messages
        private readonly int _maxWorkers;

        // The maximum number of workers that can be left to idle (not busy but allocated)
        private readonly int _maxIdleWorkers;

        // The number of allocated workers (currently running)
        private int _allocatedWorkers;

        // The number of workers that are *actually* doing work
        private int _busyWorkers;

        // Determines if the broker was disposed and should stop doing all work.
        private bool _disposed;

        public MessageBroker(IPerformanceCounterManager performanceCounterManager)
            : this(performanceCounterManager, 3 * Environment.ProcessorCount, Environment.ProcessorCount)
        {
        }

        public MessageBroker(IPerformanceCounterManager performanceCounterManager, int maxWorkers, int maxIdleWorkers)
        {
            _counters = performanceCounterManager;
            _maxWorkers = maxWorkers;
            _maxIdleWorkers = maxIdleWorkers;
        }

        public TraceSource Trace
        {
            get;
            set;
        }

        public int AllocatedWorkers
        {
            get
            {
                return _allocatedWorkers;
            }
        }

        public int BusyWorkers
        {
            get
            {
                return _busyWorkers;
            }
        }

        public void Schedule(ISubscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            if (_disposed)
            {
                // Don't queue up new work if we've disposed the broker
                return;
            }

            if (subscription.SetQueued())
            {
                lock (_queue)
                {
                    _queue.Enqueue(subscription);
                    Monitor.Pulse(_queue);
                    AddWorker();
                }
            }
        }

        private void AddWorker()
        {
            // Only create a new worker if everyone is busy (up to the max)
            if (_allocatedWorkers < _maxWorkers)
            {
                if (_allocatedWorkers == _busyWorkers)
                {
                    _counters.MessageBusAllocatedWorkers.RawValue = Interlocked.Increment(ref _allocatedWorkers);

                    Trace.TraceEvent(TraceEventType.Verbose, 0, "Creating a worker, allocated={0}, busy={1}", _allocatedWorkers, _busyWorkers);

                    ThreadPool.QueueUserWorkItem(ProcessWork);
                }
                else
                {
                    Trace.TraceEvent(TraceEventType.Verbose, 0, "No need to add a worker because all allocated workers are not busy, allocated={0}, busy={1}", _allocatedWorkers, _busyWorkers);
                }
            }
            else
            {
                Trace.TraceEvent(TraceEventType.Verbose, 0, "Already at max workers, allocated={0}, busy={1}", _allocatedWorkers, _busyWorkers);
            }
        }

        private void ProcessWork(object state)
        {
            Task pumpTask = PumpAsync();

            if (pumpTask.IsCompleted)
            {
                ProcessWorkSync(pumpTask);
            }
            else
            {
                ProcessWorkAsync(pumpTask);
            }

        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to avoid user code taking the process down.")]
        private void ProcessWorkSync(Task pumpTask)
        {
            try
            {
                pumpTask.Wait();
            }
            catch (Exception ex)
            {
                Trace.TraceEvent(TraceEventType.Error, 0, "Failed to process work - " + ex.GetBaseException());
            }
            finally
            {
                // After the pump runs decrement the number of workers in flight
                _counters.MessageBusAllocatedWorkers.RawValue = Interlocked.Decrement(ref _allocatedWorkers);
            }
        }

        private void ProcessWorkAsync(Task pumpTask)
        {
            pumpTask.ContinueWith(task =>
            {
                // After the pump runs decrement the number of workers in flight
                _counters.MessageBusAllocatedWorkers.RawValue = Interlocked.Decrement(ref _allocatedWorkers);

                if (task.IsFaulted)
                {
                    Trace.TraceEvent(TraceEventType.Error, 0, "Failed to process work - " + task.Exception.GetBaseException());
                }
            });
        }

        private Task PumpAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            PumpImpl(tcs);
            return tcs.Task;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to avoid user code taking the process down.")]
        private void PumpImpl(TaskCompletionSource<object> taskCompletionSource, ISubscription subscription = null)
        {

        Process:
            // If we were doing work before and now we've been disposed just kill this worker early
            if (_disposed)
            {
                taskCompletionSource.TrySetResult(null);
                return;
            }

            Debug.Assert(_allocatedWorkers <= _maxWorkers, "How did we pass the max?");

            // If we're withing the acceptable limit of idleness, just keep running
            int idleWorkers = _allocatedWorkers - _busyWorkers;

            if (subscription != null || idleWorkers <= _maxIdleWorkers)
            {
                // We already have a subscription doing work so skip the queue
                if (subscription == null)
                {
                    lock (_queue)
                    {
                        while (_queue.Count == 0)
                        {
                            Monitor.Wait(_queue);

                            // When disposing, all workers are pulsed so that they can quit
                            // if they're waiting for things to do (idle)
                            if (_disposed)
                            {
                                taskCompletionSource.TrySetResult(null);
                                return;
                            }
                        }

                        subscription = _queue.Dequeue();
                    }
                }

                _counters.MessageBusBusyWorkers.RawValue = Interlocked.Increment(ref _busyWorkers);

                Task workTask = subscription.Work();

                if (workTask.IsCompleted)
                {
                    try
                    {
                        workTask.Wait();

                        goto Process;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceEvent(TraceEventType.Error, 0, "Work failed for " + subscription.Identity + ": " + ex.GetBaseException());

                        goto Process;
                    }
                    finally
                    {
                        if (!subscription.UnsetQueued() || workTask.IsFaulted)
                        {
                            // If we don't have more work to do just make the subscription null
                            subscription = null;
                        }

                        _counters.MessageBusBusyWorkers.RawValue = Interlocked.Decrement(ref _busyWorkers);

                        Debug.Assert(_busyWorkers >= 0, "The number of busy workers has somehow gone negative");
                    }
                }
                else
                {
                    PumpImplAsync(workTask, subscription, taskCompletionSource);
                }
            }
            else
            {
                taskCompletionSource.TrySetResult(null);
            }
        }

        private void PumpImplAsync(Task workTask, ISubscription subscription, TaskCompletionSource<object> taskCompletionSource)
        {
            // Async path
            workTask.ContinueWith(task =>
            {
                bool moreWork = subscription.UnsetQueued();

                _counters.MessageBusBusyWorkers.RawValue = Interlocked.Decrement(ref _busyWorkers);

                Debug.Assert(_busyWorkers >= 0, "The number of busy workers has somehow gone negative");

                if (task.IsFaulted)
                {
                    Trace.TraceEvent(TraceEventType.Error, 0, "Work failed for " + subscription.Identity + ": " + task.Exception.GetBaseException());
                }

                if (moreWork && !task.IsFaulted)
                {
                    PumpImpl(taskCompletionSource, subscription);
                }
                else
                {
                    // Don't reference the subscription anymore
                    subscription = null;

                    PumpImpl(taskCompletionSource);
                }
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;

                    Trace.TraceEvent(TraceEventType.Verbose, 0, "Dispoing the broker");

                    // Wait for all threads to stop working
                    WaitForDrain();

                    Trace.TraceEvent(TraceEventType.Verbose, 0, "Disposed the broker");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void WaitForDrain()
        {
            while (_allocatedWorkers > 0)
            {
                lock (_queue)
                {
                    // Tell all workers we're done
                    Monitor.PulseAll(_queue);
                }

                Thread.Sleep(250);
            }
        }
    }
}
