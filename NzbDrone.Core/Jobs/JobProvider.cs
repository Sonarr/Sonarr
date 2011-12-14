//https://github.com/kayone/NzbDrone/blob/master/NzbDrone.Core/Providers/Jobs/JobProvider.cs

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog;
using Ninject;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Jobs
{
    /// <summary>
    /// Provides a background task runner, tasks could be queue either by the scheduler using QueueScheduled()
    /// or by explicitly calling QueueJob(type,int)
    /// </summary>
    public class JobProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;
        private readonly NotificationProvider _notificationProvider;
        private readonly IList<IJob> _jobs;

        private Thread _jobThread;
        public Stopwatch StopWatch { get; private set; }

        private readonly object executionLock = new object();
        private readonly List<JobQueueItem> _queue = new List<JobQueueItem>();

        private ProgressNotification _notification;


        [Inject]
        public JobProvider(IDatabase database, NotificationProvider notificationProvider, IList<IJob> jobs)
        {
            StopWatch = new Stopwatch();
            _database = database;
            _notificationProvider = notificationProvider;
            _jobs = jobs;
            ResetThread();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobProvider"/> class. by AutoMoq
        /// </summary>
        /// <remarks>Should only be used by AutoMoq</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public JobProvider() { }

        /// <summary>
        /// Gets the active queue.
        /// </summary>
        public List<JobQueueItem> Queue
        {
            get
            {
                return _queue;
            }
        }

        public virtual List<JobDefinition> All()
        {
            return _database.Fetch<JobDefinition>().ToList();
        }

        public virtual void Initialize()
        {
            var currentJobs = All();
            logger.Debug("Initializing jobs. Available: {0} Existing:{1}", _jobs.Count(), currentJobs.Count);

            foreach (var currentJob in currentJobs)
            {
                if (!_jobs.Any(c => c.GetType().ToString() == currentJob.TypeName))
                {
                    logger.Debug("Removing job from database '{0}'", currentJob.Name);
                    _database.Delete(currentJob);
                }
            }

            foreach (var job in _jobs)
            {
                var jobDefinition = currentJobs.SingleOrDefault(c => c.TypeName == job.GetType().ToString());

                if (jobDefinition == null)
                {
                    jobDefinition = new JobDefinition();
                    jobDefinition.TypeName = job.GetType().ToString();
                    jobDefinition.LastExecution = DateTime.Now;
                }

                jobDefinition.Enable = job.DefaultInterval > 0;
                jobDefinition.Name = job.Name;
                jobDefinition.Interval = job.DefaultInterval;

                SaveDefinition(jobDefinition);
            }
        }

        /// <summary>
        /// Adds/Updates definitions for a job
        /// </summary>
        /// <param name="definitions">Settings to be added/updated</param>
        public virtual void SaveDefinition(JobDefinition definitions)
        {
            if (definitions.Id == 0)
            {
                logger.Trace("Adding job definitions for {0}", definitions.Name);
                _database.Insert(definitions);
            }
            else
            {
                logger.Trace("Updating job definitions for {0}", definitions.Name);
                _database.Update(definitions);
            }
        }

        public virtual void QueueScheduled()
        {
            lock (executionLock)
            {
                VerifyThreadTime();

                if (_jobThread.IsAlive)
                {
                    logger.Trace("Queue is already running. Ignoring scheduler's request.");
                    return;
                }
            }

            var pendingJobTypes = All().Where(
                t => t.Enable &&
                     (DateTime.Now - t.LastExecution) > TimeSpan.FromMinutes(t.Interval)
                ).Select(c => _jobs.Where(t => t.GetType().ToString() == c.TypeName).Single().GetType()).ToList();


            pendingJobTypes.ForEach(jobType => QueueJob(jobType));
            logger.Trace("{0} Scheduled tasks have been added to the queue", pendingJobTypes.Count);
        }

        public virtual void QueueJob(Type jobType, int targetId = 0, int secondaryTargetId = 0)
        {
            var queueItem = new JobQueueItem
            {
                JobType = jobType,
                TargetId = targetId,
                SecondaryTargetId = secondaryTargetId
            };

            logger.Debug("Attempting to queue {0}", queueItem);

            lock (executionLock)
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
                                    job = Queue.First();
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
            var jobImplementation = _jobs.Where(t => t.GetType() == queueItem.JobType).SingleOrDefault();
            if (jobImplementation == null)
            {
                logger.Error("Unable to locate implementation for '{0}'. Make sure it is properly registered.", queueItem.JobType);
                return;
            }

            var settings = All().Where(j => j.TypeName == queueItem.JobType.ToString()).Single();

            using (_notification = new ProgressNotification(jobImplementation.Name))
            {
                try
                {
                    logger.Debug("Starting {0}. Last execution {1}", queueItem, settings.LastExecution);

                    var sw = Stopwatch.StartNew();

                    _notificationProvider.Register(_notification);
                    jobImplementation.Start(_notification, queueItem.TargetId, queueItem.SecondaryTargetId);
                    _notification.Status = ProgressNotificationStatus.Completed;

                    settings.LastExecution = DateTime.Now;
                    settings.Success = true;

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

                    settings.LastExecution = DateTime.Now;
                    settings.Success = false;
                }
            }

            //Only update last execution status if was triggered by the scheduler
            if (queueItem.TargetId == 0)
            {
                SaveDefinition(settings);
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