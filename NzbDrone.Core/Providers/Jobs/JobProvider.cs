using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Ninject;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers.Jobs
{
    public class JobProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;
        private readonly NotificationProvider _notificationProvider;
        private readonly IList<IJob> _jobs;

        private static readonly object ExecutionLock = new object();
        private Thread _jobThread;
        private static bool _isRunning;
        public static readonly List<Tuple<Type, Int32>> Queue = new List<Tuple<Type, int>>();



        private ProgressNotification _notification;

        [Inject]
        public JobProvider(IDatabase database, NotificationProvider notificationProvider, IList<IJob> jobs)
        {
            _database = database;
            _notificationProvider = notificationProvider;
            _jobs = jobs;
        }

        public JobProvider() { }

        /// <summary>
        /// Returns a list of all registered jobs
        /// </summary>
        /// <returns></returns>
        public virtual List<JobDefinition> All()
        {
            return _database.Fetch<JobDefinition>().ToList();
        }

        /// <summary>
        /// Creates/Updates definitions for a job
        /// </summary>
        /// <param name="definitions">Settings to be created/updated</param>
        public virtual void SaveSettings(JobDefinition definitions)
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
        /// Iterates through all registered jobs and executed any that are due for an execution.
        /// </summary>
        public virtual void RunScheduled()
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
        /// Starts the execution of a job asynchronously
        /// </summary>
        /// <param name="jobType">Type of the job that should be executed.</param>
        /// <param name="targetId">The targetId could be any Id parameter eg. SeriesId. it will be passed to the job implementation
        /// to allow it to filter it's target of execution.</param>
        /// <remarks>Job is only added to the queue if same job with the same targetId doesn't already exist in the queue.</remarks>
        public virtual void QueueJob(Type jobType, int targetId = 0)
        {
            Logger.Debug("Adding [{0}:{1}] to the queue", jobType.Name, targetId);

            lock (ExecutionLock)
            {
                lock (Queue)
                {
                    var queueTuple = new Tuple<Type, int>(jobType, targetId);

                    if (!Queue.Contains(queueTuple))
                    {
                        Queue.Add(queueTuple);
                        Logger.Trace("Job [{0}:{1}] added to the queue", jobType.Name, targetId);

                    }
                    else
                    {
                        Logger.Info("[{0}:{1}] already exists in job queue. Skipping.", jobType.Name, targetId);
                    }
                }

                if (_isRunning)
                {
                    Logger.Trace("Queue is already running. No need to start it up.");
                    return;
                }
                _isRunning = true;
            }

            if (_jobThread == null || !_jobThread.IsAlive)
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
                    }
                };

                _jobThread = new Thread(starter) { Name = "JobQueueThread" };
                _jobThread.Start();

            }
            else
            {
                Logger.Error("Execution lock has fucked up. Thread still active. Ignoring request.");
            }

        }

        /// <summary>
        /// Starts processing of queue.
        /// </summary>
        private void ProcessQueue()
        {
            do
            {
                Tuple<Type, int> job = null;

                try
                {
                    lock (Queue)
                    {
                        if (Queue.Count != 0)
                        {
                            job = Queue.First();
                        }
                    }

                    if (job != null)
                    {
                        Execute(job.Item1, job.Item2);
                    }

                }
                catch (Exception e)
                {
                    Logger.FatalException("An error has occurred while processing queued job.", e);
                }
                finally
                {
                    if (job != null)
                    {
                        Queue.Remove(job);
                    }
                }

            } while (Queue.Count != 0);

            Logger.Trace("Finished processing jobs in the queue.");

            return;
        }

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <param name="jobType">Type of the job that should be executed</param>
        /// <param name="targetId">The targetId could be any Id parameter eg. SeriesId. it will be passed to the timer implementation
        /// to allow it to filter it's target of execution</param>
        private void Execute(Type jobType, int targetId = 0)
        {
            var jobImplementation = _jobs.Where(t => t.GetType() == jobType).FirstOrDefault();
            if (jobImplementation == null)
            {
                Logger.Error("Unable to locate implementation for '{0}'. Make sure it is properly registered.", jobType);
                return;
            }

            var settings = All().Where(j => j.TypeName == jobType.ToString()).FirstOrDefault();

            using (_notification = new ProgressNotification(jobImplementation.Name))
            {
                try
                {
                    Logger.Debug("Starting '{0}' job. Last execution {1}", settings.Name, settings.LastExecution);

                    var sw = Stopwatch.StartNew();

                    _notificationProvider.Register(_notification);
                    jobImplementation.Start(_notification, targetId);
                    _notification.Status = ProgressNotificationStatus.Completed;

                    settings.LastExecution = DateTime.Now;
                    settings.Success = true;

                    sw.Stop();
                    Logger.Debug("Job '{0}' successfully completed in {1}.{2} seconds.", jobImplementation.Name, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds / 100,
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

            if (targetId == 0)
            {
                SaveSettings(settings);
            }
        }

        /// <summary>
        /// Initializes jobs in the database using the IJob instances that are
        /// registered in CentralDispatch
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

                    SaveSettings(settings);
                }
            }
        }

        /// <summary>
        /// Gets the next scheduled run time for the job
        /// (Estimated due to schedule timer)
        /// </summary>
        /// <returns>DateTime of next scheduled job execution</returns>
        public virtual DateTime NextScheduledRun(Type jobType)
        {
            var job = All().Where(t => t.TypeName == jobType.ToString()).FirstOrDefault();

            if (job == null)
                return DateTime.Now;

            return job.LastExecution.AddMinutes(job.Interval);
        }
    }
}