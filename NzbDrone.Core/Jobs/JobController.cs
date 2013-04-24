using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public interface IJobController
    {
        bool IsProcessing { get; }
        IEnumerable<JobQueueItem> Queue { get; }
        void EnqueueScheduled();
        void Enqueue(Type jobType, dynamic options = null, JobQueueItem.JobSourceType source = JobQueueItem.JobSourceType.User);
        bool Enqueue(string jobTypeString);
    }

    public class JobController : IJobController, IHandle<ApplicationShutdownRequested>
    {
        private readonly NotificationProvider _notificationProvider;
        private readonly IEnumerable<IJob> _jobs;
        private readonly IJobRepository _jobRepository;
        private readonly Logger _logger;

        private readonly BlockingCollection<JobQueueItem> _queue = new BlockingCollection<JobQueueItem>();

        private ProgressNotification _notification;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public JobController(NotificationProvider notificationProvider, IEnumerable<IJob> jobs, IJobRepository jobRepository, Logger logger)
        {
            _notificationProvider = notificationProvider;
            _jobs = jobs;
            _jobRepository = jobRepository;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(ProcessQueue, _cancellationTokenSource.Token);
        }

        public bool IsProcessing { get; private set; }

        public IEnumerable<JobQueueItem> Queue
        {
            get
            {
                return _queue;
            }
        }

        public void EnqueueScheduled()
        {
            if (IsProcessing)
            {
                _logger.Trace("Queue is already running. Ignoring scheduler request.");
                return;
            }

            var pendingJobs = _jobRepository.GetPendingJobs()
                .Select(c => _jobs.Single(t => t.GetType().ToString() == c.Type)
                .GetType()).ToList();


            pendingJobs.ForEach(jobType => Enqueue(jobType, source: JobQueueItem.JobSourceType.Scheduler));
            _logger.Trace("{0} Scheduled tasks have been added to the queue", pendingJobs.Count);
        }

        public void Enqueue(Type jobType, dynamic options = null, JobQueueItem.JobSourceType source = JobQueueItem.JobSourceType.User)
        {
            IsProcessing = true;

            var queueItem = new JobQueueItem
            {
                JobType = jobType,
                Options = options,
                Source = source
            };

            _logger.Debug("Attempting to queue {0}", queueItem);

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

        }

        public bool Enqueue(string jobTypeString)
        {
            var type = Type.GetType(jobTypeString);

            if (type == null)
                return false;

            Enqueue(type);
            return true;
        }

        private void ProcessQueue()
        {
            while (true)
            {
                try
                {
                    IsProcessing = false;
                    var item = _queue.Take();
                    Execute(item);
                }
                catch (ThreadAbortException e)
                {
                    _logger.Warn(e.Message);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Error has occurred in queue processor thread", e);
                }
            }
        }

        private void Execute(JobQueueItem queueItem)
        {
            IsProcessing = true;

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

        public void Handle(ApplicationShutdownRequested message)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}