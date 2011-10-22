//https://github.com/kayone/NzbDrone/blob/master/NzbDrone.Core/Providers/Jobs/JobProvider.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;
using PetaPoco;
using ThreadState = System.Threading.ThreadState;

namespace NzbDrone.Core.Providers.Jobs
{
    /// <summary>
    /// Provides a background task runner, tasks could be queue either by the scheduler using QueueScheduled()
    /// or by explicitly calling QueueJob(type,int)
    /// </summary>
    public class JobProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;
        private readonly NotificationProvider _notificationProvider;
        private readonly IList<IJob> _jobs;

        private static readonly object ExecutionLock = new object();
        private Thread _jobThread;
        private static bool _isRunning;

        private static readonly List<JobQueueItem> _queue = new List<JobQueueItem>();

        private ProgressNotification _notification;

        [Inject]
        public JobProvider(IDatabase database, NotificationProvider notificationProvider, IList<IJob> jobs)
        {
            _database = database;
            _notificationProvider = notificationProvider;
            _jobs = jobs;
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
        public static List<JobQueueItem> Queue
        {
            get
            {
                return _queue;
            }
        }

        /// <summary>
        /// Returns a list of all registered jobs
        /// </summary>
        public virtual List<JobDefinition> All()
        {
            return _database.Fetch<JobDefinition>().ToList();
        }

        /// <summary>
        /// Adds/Updates definitions for a job
        /// </summary>
        /// <param name="definitions">Settings to be added/updated</param>
        public virtual void SaveDefinition(JobDefinition definitions)
        {
            if (definitions.Id == 0)
            {
                Logger.Trace("Adding job definitions for {0}", definitions.Name);
                _database.Insert(definitions);
            }
            else
            {
                Logger.Trace("Updating job definitions for {0}", definitions.Name);
                _database.Update(definitions);
            }
        }

        /// <summary>
        /// Iterates through all registered jobs and queues any that are due for an execution.
        /// </summary>
        /// <remarks>Will ignore request if queue is already running.</remarks>
        public virtual void QueueScheduled()
        {
            lock (ExecutionLock)
            {
                if (_isRunning)
                {
                    Logger.Trace("Queue is already running. Ignoring scheduler's request.");
                    return;
                }
            }

            var counter = 0;

            var pendingJobs = All().Where(
                t => t.Enable &&
                     (DateTime.Now - t.LastExecution) > TimeSpan.FromMinutes(t.Interval)
                ).Select(c => _jobs.Where(t => t.GetType().ToString() == c.TypeName).Single());

            foreach (var job in pendingJobs)
            {
                QueueJob(job.GetType());
                counter++;
            }

            Logger.Trace("{0} Scheduled tasks have been added to the queue", counter);
        }

        /// <summary>
        /// Queues the execution of a job asynchronously
        /// </summary>
        /// <param name="jobType">Type of the job that should be queued.</param>
        /// <param name="targetId">The targetId could be any Id parameter eg. SeriesId. it will be passed to the job implementation
        /// to allow it to filter it's target of execution.</param>
        /// /// <param name="secondaryTargetId">The secondaryTargetId could be any Id parameter eg. SeasonNumber. it will be passed to 
        /// the timer implementation to further allow it to filter it's target of execution</param>
        /// <remarks>Job is only added to the queue if same job with the same targetId doesn't already exist in the queue.</remarks>
        public virtual void QueueJob(Type jobType, int targetId = 0, int secondaryTargetId = 0)
        {
            Logger.Debug("Adding [{0}:{1}] to the queue", jobType.Name, targetId);

            lock (ExecutionLock)
            {
                lock (Queue)
                {
                    var queueItem = new JobQueueItem
                                        {
                                            JobType = jobType,
                                            TargetId = targetId,
                                            SecondaryTargetId = secondaryTargetId
                                        };

                    if (!Queue.Contains(queueItem))
                    {
                        Queue.Add(queueItem);
                        Logger.Trace("Job [{0}:{1}] added to the queue", jobType.Name, targetId);

                    }
                    else
                    {
                        Logger.Info("[{0}:{1}] already exists in the queue. Skipping.", jobType.Name, targetId);
                    }
                }

                if (_isRunning)
                {
                    Logger.Trace("Queue is already running. No need to start it up.");
                    return;
                }
                _isRunning = true;
            }

            if (_jobThread == null || _jobThread.ThreadState != ThreadState.Running)
            {
                Logger.Trace("Initializing queue processor thread");

                ThreadStart starter = () =>
                {
                    try
                    {
                        ProcessQueue();
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorException("Error has occurred in queue processor thread", e);
                    }
                    finally
                    {
                        _isRunning = false;
                        _jobThread.Abort();

                    }
                };

                _jobThread = new Thread(starter) { Name = "JobQueueThread" };
                _jobThread.Start();

            }
            else
            {
                var messge = "Job Thread is null";

                if (_jobThread != null)
                {
                    messge = "Job Thread State: " + _jobThread.ThreadState;
                }

                Logger.Error("Execution lock has fucked up. {0}. Ignoring request.", messge);
            }

        }

        /// <summary>
        /// Starts processing of queue synchronously.
        /// </summary>
        private void ProcessQueue()
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
                                Queue.Remove(job);
                            }
                        }

                        if (job != null)
                        {
                            Execute(job.JobType, job.TargetId, job.SecondaryTargetId);
                        }

                    }
                    catch (Exception e)
                    {
                        Logger.FatalException("An error has occurred while processing queued job.", e);
                    }
                }

            } while (Queue.Count != 0);

            Logger.Trace("Finished processing jobs in the queue.");

            return;
        }

        /// <summary>
        /// Executes the job synchronously
        /// </summary>
        /// <param name="jobType">Type of the job that should be executed</param>
        /// <param name="targetId">The targetId could be any Id parameter eg. SeriesId. it will be passed to the timer implementation
        /// to allow it to filter it's target of execution</param>
        /// /// <param name="secondaryTargetId">The secondaryTargetId could be any Id parameter eg. SeasonNumber. it will be passed to 
        /// the timer implementation to further allow it to filter it's target of execution</param>
        private void Execute(Type jobType, int targetId = 0, int secondaryTargetId = 0)
        {
            var jobImplementation = _jobs.Where(t => t.GetType() == jobType).Single();
            if (jobImplementation == null)
            {
                Logger.Error("Unable to locate implementation for '{0}'. Make sure it is properly registered.", jobType);
                return;
            }

            var settings = All().Where(j => j.TypeName == jobType.ToString()).Single();

            using (_notification = new ProgressNotification(jobImplementation.Name))
            {
                try
                {
                    Logger.Debug("Starting '{0}' job. Last execution {1}", settings.Name, settings.LastExecution);

                    var sw = Stopwatch.StartNew();

                    _notificationProvider.Register(_notification);
                    jobImplementation.Start(_notification, targetId, secondaryTargetId);
                    _notification.Status = ProgressNotificationStatus.Completed;

                    settings.LastExecution = DateTime.Now;
                    settings.Success = true;

                    sw.Stop();
                    Logger.Debug("Job '{0}' successfully completed in {1:0}.{2} seconds.", jobImplementation.Name, sw.Elapsed.TotalSeconds, sw.Elapsed.Milliseconds / 100,
                                sw.Elapsed.Seconds);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while executing job " + jobImplementation.Name, e);
                    _notification.Status = ProgressNotificationStatus.Failed;
                    _notification.CurrentMessage = jobImplementation.Name + " Failed.";

                    settings.LastExecution = DateTime.Now;
                    settings.Success = false;
                }
            }

            //Only update last execution status if was triggered by the scheduler
            if (targetId == 0)
            {
                SaveDefinition(settings);
            }
        }

        /// <summary>
        /// Initializes jobs in the database using the IJob instances that are
        /// registered using ninject
        /// </summary>
        public virtual void Initialize()
        {
            Logger.Debug("Initializing jobs. Count {0}", _jobs.Count());
            var currentTimer = All();

            foreach (var timer in _jobs)
            {
                var timerProviderLocal = timer;
                if (!currentTimer.Exists(c => c.TypeName == timerProviderLocal.GetType().ToString()))
                {
                    var settings = new JobDefinition
                                       {
                                           Enable = timerProviderLocal.DefaultInterval > 0,
                                           TypeName = timer.GetType().ToString(),
                                           Name = timerProviderLocal.Name,
                                           Interval = timerProviderLocal.DefaultInterval,
                                           LastExecution = new DateTime(2000, 1, 1)
                                       };

                    SaveDefinition(settings);
                }
            }
        }

        /// <summary>
        /// Gets the next scheduled run time for a specific job
        /// (Estimated due to schedule timer)
        /// </summary>
        /// <returns>DateTime of next scheduled job execution</returns>
        public virtual DateTime NextScheduledRun(Type jobType)
        {
            var job = All().Where(t => t.TypeName == jobType.ToString()).Single();
            return job.LastExecution.AddMinutes(job.Interval);
        }
    }
}