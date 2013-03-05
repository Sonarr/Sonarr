using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public interface IJobController
    {
        bool IsProcessing { get; }
        IEnumerable<JobQueueItem> Queue { get; }
        void QueueScheduled();
        void QueueJob(Type jobType, dynamic options = null, JobQueueItem.JobSourceType source = JobQueueItem.JobSourceType.User);
        bool QueueJob(string jobTypeString);
    }

    public class JobController : IJobController
    {
        private readonly NotificationProvider _notificationProvider;
        private readonly IEnumerable<IJob> _jobs;
        private readonly IJobRepository _jobRepository;
        private readonly Logger _logger;

        private Thread _jobThread;



        private readonly object _executionLock = new object();
        private readonly BlockingCollection<JobQueueItem> _queue = new BlockingCollection<JobQueueItem>();

        private ProgressNotification _notification;

        public JobController(NotificationProvider notificationProvider, IEnumerable<IJob> jobs, IJobRepository jobRepository, Logger logger)
        {
            _notificationProvider = notificationProvider;
            _jobs = jobs;
            _jobRepository = jobRepository;
            _logger = logger;
            ResetThread();
        }


        public bool IsProcessing { get; private set; }

        public IEnumerable<JobQueueItem> Queue
        {
            get
            {
                return _queue;
            }
        }

        public virtual void QueueScheduled()
        {
            lock (_executionLock)
            {
                if (_jobThread.IsAlive)
                {
                    _logger.Trace("Queue is already running. Ignoring scheduler's request.");
                    return;
                }
            }

            var pendingJobs = _jobRepository.GetPendingJobs()
                .Select(c => _jobs.Single(t => t.GetType().ToString() == c.TypeName)
                .GetType()).ToList();


            pendingJobs.ForEach(jobType => QueueJob(jobType, source: JobQueueItem.JobSourceType.Scheduler));
            _logger.Trace("{0} Scheduled tasks have been added to the queue", pendingJobs.Count);
        }

        public virtual void QueueJob(Type jobType, dynamic options = null, JobQueueItem.JobSourceType source = JobQueueItem.JobSourceType.User)
        {
            IsProcessing = true;

            var queueItem = new JobQueueItem
            {
                JobType = jobType,
                Options = options,
                Source = source
            };

            _logger.Debug("Attempting to queue {0}", queueItem);

            lock (_executionLock)
            {
                lock (_queue)
                {
                    if (!_queue.Contains(queueItem))
                    {
                        _queue.Add(queueItem);
                        _logger.Trace("Job {0} added to the queue. current items in queue: {1}", queueItem, _queue.Count);
                    }
                    else
                    {
                        _logger.Info("{0} already exists in the queue. Skipping. current items in queue: {1}", queueItem, _queue.Count);
                    }
                }

                if (_jobThread.IsAlive)
                {
                    _logger.Trace("Queue is already running. No need to start it up.");
                    return;
                }

                ResetThread();
                _jobThread.Start();
            }

        }

        public virtual bool QueueJob(string jobTypeString)
        {
            var type = Type.GetType(jobTypeString);

            if (type == null)
                return false;

            QueueJob(type);
            return true;
        }

        private void ProcessQueue()
        {
            try
            {
                while (true)
                {
                    IsProcessing = false;
                    var item = _queue.Take();
                    IsProcessing = true;
                    
                    try
                    {
                        Execute(item);
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        _logger.FatalException("An error has occurred while executing job.", e);
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                _logger.Warn(e.Message);
            }
            catch (Exception e)
            {
                _logger.ErrorException("Error has occurred in queue processor thread", e);
            }
            finally
            {
                _logger.Trace("Finished processing jobs in the queue.");
            }
        }

        private void Execute(JobQueueItem queueItem)
        {
            var jobImplementation = _jobs.SingleOrDefault(t => t.GetType() == queueItem.JobType);
            if (jobImplementation == null)
            {
                _logger.Error("Unable to locate implementation for '{0}'. Make sure it is properly registered.", queueItem.JobType);
                return;
            }

            var jobDefinition = _jobRepository.GetDefinition(queueItem.JobType);
            using (_notification = new ProgressNotification(jobImplementation.Name))
            {
                try
                {
                    _logger.Debug("Starting {0}. Last execution {1}", queueItem, jobDefinition.LastExecution);

                    var sw = Stopwatch.StartNew();

                    _notificationProvider.Register(_notification);
                    jobImplementation.Start(_notification, queueItem.Options);
                    _notification.Status = ProgressNotificationStatus.Completed;

                    jobDefinition.LastExecution = DateTime.Now;
                    jobDefinition.Success = true;

                    sw.Stop();
                    _logger.Debug("Job {0} successfully completed in {1:0}.{2} seconds.", queueItem, sw.Elapsed.TotalSeconds, sw.Elapsed.Milliseconds / 100,
                                 sw.Elapsed.Seconds);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error has occurred while executing job [" + jobImplementation.Name + "].", e);
                    _notification.Status = ProgressNotificationStatus.Failed;
                    _notification.CurrentMessage = jobImplementation.Name + " Failed.";

                    jobDefinition.LastExecution = DateTime.Now;
                    jobDefinition.Success = false;
                }
            }

            //Only update last execution status if was triggered by the scheduler
            if (queueItem.Options == null)
            {
                _jobRepository.Update(jobDefinition);
            }
        }


        private void ResetThread()
        {
            if (_jobThread != null)
            {
                _jobThread.Abort();
            }

            _logger.Trace("resetting queue processor thread");
            _jobThread = new Thread(ProcessQueue) { Name = "JobQueueThread" };
        }
    }
}