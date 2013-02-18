using System;
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
        Stopwatch StopWatch { get; }
        List<JobQueueItem> Queue { get; }
        void QueueScheduled();
        void QueueJob(Type jobType, dynamic options = null, JobQueueItem.JobSourceType source = JobQueueItem.JobSourceType.User);
        bool QueueJob(string jobTypeString);
    }

    public class JobController : IJobController
    {
        private readonly NotificationProvider _notificationProvider;
        private readonly IEnumerable<IJob> _jobs;
        private readonly IJobRepository _jobRepository;
        private readonly Logger logger;

        private Thread _jobThread;
        public Stopwatch StopWatch { get; private set; }

        private readonly object _executionLock = new object();
        private readonly List<JobQueueItem> _queue = new List<JobQueueItem>();

        private ProgressNotification _notification;


        public JobController(NotificationProvider notificationProvider, IEnumerable<IJob> jobs, IJobRepository jobRepository, Logger logger)
        {
            StopWatch = new Stopwatch();
            _notificationProvider = notificationProvider;
            _jobs = jobs;
            _jobRepository = jobRepository;
            this.logger = logger;
            ResetThread();
        }

        public List<JobQueueItem> Queue
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
                VerifyThreadTime();

                if (_jobThread.IsAlive)
                {
                    logger.Trace("Queue is already running. Ignoring scheduler's request.");
                    return;
                }
            }

            var pendingJobs = _jobRepository.GetPendingJobs()
                .Select(c => _jobs.Single(t => t.GetType().ToString() == c.TypeName)
                .GetType()).ToList();


            pendingJobs.ForEach(jobType => QueueJob(jobType, source: JobQueueItem.JobSourceType.Scheduler));
            logger.Trace("{0} Scheduled tasks have been added to the queue", pendingJobs.Count);
        }

        public virtual void QueueJob(Type jobType, dynamic options = null, JobQueueItem.JobSourceType source = JobQueueItem.JobSourceType.User)
        {
            var queueItem = new JobQueueItem
            {
                JobType = jobType,
                Options = options,
                Source = source
            };

            logger.Debug("Attempting to queue {0}", queueItem);

            lock (_executionLock)
            {
                VerifyThreadTime();

                lock (Queue)
                {
                    if (!Queue.Contains(queueItem))
                    {
                        Queue.Add(queueItem);
                        logger.Trace("Job {0} added to the queue. current items in queue: {1}", queueItem, Queue.Count);
                    }
                    else
                    {
                        logger.Info("{0} already exists in the queue. Skipping. current items in queue: {1}", queueItem, Queue.Count);
                    }
                }

                if (_jobThread.IsAlive)
                {
                    logger.Trace("Queue is already running. No need to start it up.");
                    return;
                }

                ResetThread();
                StopWatch = Stopwatch.StartNew();
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
                do
                {
                    using (NestedDiagnosticsContext.Push(Guid.NewGuid().ToString()))
                    {
                        try
                        {
                            JobQueueItem job = null;

                            lock (Queue)
                            {
                                if (Queue.Count != 0)
                                {
                                    job = Queue.OrderBy(c => c.Source).First();
                                    logger.Trace("Popping {0} from the queue.", job);
                                    Queue.Remove(job);
                                }
                            }

                            if (job != null)
                            {
                                Execute(job);
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            logger.FatalException("An error has occurred while executing job.", e);
                        }
                    }

                } while (Queue.Count != 0);
            }
            catch (ThreadAbortException e)
            {
                logger.Warn(e.Message);
            }
            catch (Exception e)
            {
                logger.ErrorException("Error has occurred in queue processor thread", e);
            }
            finally
            {
                StopWatch.Stop();
                logger.Trace("Finished processing jobs in the queue.");
            }
        }

        private void Execute(JobQueueItem queueItem)
        {
            var jobImplementation = _jobs.SingleOrDefault(t => t.GetType() == queueItem.JobType);
            if (jobImplementation == null)
            {
                logger.Error("Unable to locate implementation for '{0}'. Make sure it is properly registered.", queueItem.JobType);
                return;
            }

            var jobDefinition = _jobRepository.GetDefinition(queueItem.JobType);
            using (_notification = new ProgressNotification(jobImplementation.Name))
            {
                try
                {
                    logger.Debug("Starting {0}. Last execution {1}", queueItem, jobDefinition.LastExecution);

                    var sw = Stopwatch.StartNew();

                    _notificationProvider.Register(_notification);
                    jobImplementation.Start(_notification, queueItem.Options);
                    _notification.Status = ProgressNotificationStatus.Completed;

                    jobDefinition.LastExecution = DateTime.Now;
                    jobDefinition.Success = true;

                    sw.Stop();
                    logger.Debug("Job {0} successfully completed in {1:0}.{2} seconds.", queueItem, sw.Elapsed.TotalSeconds, sw.Elapsed.Milliseconds / 100,
                                 sw.Elapsed.Seconds);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    logger.ErrorException("An error has occurred while executing job [" + jobImplementation.Name + "].", e);
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

        private void VerifyThreadTime()
        {
            if (StopWatch.Elapsed.TotalHours > 1)
            {
                logger.Warn("Thread job has been running for more than an hour. fuck it!");
                ResetThread();
            }
        }

        private void ResetThread()
        {
            if (_jobThread != null)
            {
                _jobThread.Abort();
            }

            logger.Trace("resetting queue processor thread");
            _jobThread = new Thread(ProcessQueue) { Name = "JobQueueThread" };
        }
    }
}