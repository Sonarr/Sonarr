using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class JobProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;
        private readonly NotificationProvider _notificationProvider;
        private readonly IEnumerable<IJob> _jobs;

        private static readonly object ExecutionLock = new object();
        private Thread _jobThread;
        private static bool _isRunning;

        private ProgressNotification _notification;

        public JobProvider(IRepository repository, NotificationProvider notificationProvider, IEnumerable<IJob> jobs)
        {
            _repository = repository;
            _notificationProvider = notificationProvider;
            _jobs = jobs;
        }

        public JobProvider() { }


        /// <summary>
        /// Returns a list of all registered jobs
        /// </summary>
        /// <returns></returns>
        public virtual List<JobSetting> All()
        {
            return _repository.All<JobSetting>().ToList();
        }

        /// <summary>
        /// Creates/Updates settings for a job
        /// </summary>
        /// <param name="settings">Settings to be created/updated</param>
        public virtual void SaveSettings(JobSetting settings)
        {
            if (settings.Id == 0)
            {
                Logger.Debug("Adding job settings for {0}", settings.Name);
                _repository.Add(settings);
            }
            else
            {
                Logger.Debug("Updating job settings for {0}", settings.Name);
                _repository.Update(settings);
            }
        }

        /// <summary>
        /// Iterates through all registered jobs and executed any that are due for an execution.
        /// </summary>
        /// <returns>True if ran, false if skipped</returns>
        public virtual bool RunScheduled()
        {
            lock (ExecutionLock)
            {
                if (_isRunning)
                {
                    Logger.Info("Another instance of this job is already running. Ignoring request.");
                    return false;
                }
                _isRunning = true;
            }

            try
            {

                var pendingJobs = All().Where(
                    t => t.Enable &&
                         (DateTime.Now - t.LastExecution) > TimeSpan.FromMinutes(t.Interval)
                    );

                foreach (var pendingTimer in pendingJobs)
                {
                    var timerClass = _jobs.Where(t => t.GetType().ToString() == pendingTimer.TypeName).FirstOrDefault();
                    Execute(timerClass.GetType(), 0);
                }
            }
            finally
            {
                _isRunning = false;
            }

            return true;
        }



        /// <summary>
        /// Starts the execution of a job asynchronously
        /// </summary>
        /// <param name="jobType">Type of the job that should be executed.</param>
        /// <param name="targetId">The targetId could be any Id parameter eg. SeriesId. it will be passed to the job implementation
        /// to allow it to filter it's target of execution.</param>
        /// <returns>True if ran, false if skipped</returns>
        public bool BeginExecute(Type jobType, int targetId = 0)
        {
            lock (ExecutionLock)
            {
                if (_isRunning)
                {
                    Logger.Info("Another job is already running. Ignoring request.");
                    return false;
                }
                _isRunning = true;
            }
            if (_jobThread == null || !_jobThread.IsAlive)
            {
                Logger.Trace("Initializing background thread");

                ThreadStart starter = () =>
                {
                    try
                    {
                        Execute(jobType, targetId);
                    }
                    finally
                    {
                        _isRunning = false;
                    }
                };

                _jobThread = new Thread(starter) { Name = "TimerThread", Priority = ThreadPriority.BelowNormal };
                _jobThread.Start();

            }
            else
            {
                Logger.Warn("Thread still active. Ignoring request.");
            }

            return true;
        }

        /// <summary>
        /// Executes the job
        /// </summary>
        /// <param name="jobType">Type of the job that should be executed</param>
        /// <param name="targetId">The targetId could be any Id parameter eg. SeriesId. it will be passed to the timer implementation
        /// to allow it to filter it's target of execution</param>
        private void Execute(Type jobType, int targetId = 0)
        {
            var timerClass = _jobs.Where(t => t.GetType() == jobType).FirstOrDefault();
            if (timerClass == null)
            {
                Logger.Error("Unable to locate implantation for '{0}'. Make sure its properly registered.", jobType.ToString());
                return;
            }

            var settings = All().Where(j => j.TypeName == jobType.ToString()).FirstOrDefault();

            using (_notification = new ProgressNotification(timerClass.Name))
            {
                try
                {
                    Logger.Debug("Starting job '{0}'. Last execution {1}", settings.Name, settings.LastExecution);
                    settings.LastExecution = DateTime.Now;
                    var sw = Stopwatch.StartNew();

                    _notificationProvider.Register(_notification);
                    timerClass.Start(_notification, targetId);
                    _notification.Status = ProgressNotificationStatus.Completed;

                    settings.Success = true;
                    sw.Stop();
                    Logger.Debug("Job '{0}' successfully completed in {1} seconds", timerClass.Name, sw.Elapsed.Minutes,
                                sw.Elapsed.Seconds);
                }
                catch (Exception e)
                {
                    settings.Success = false;
                    Logger.ErrorException("An error has occurred while executing timer job " + timerClass.Name, e);
                    _notification.CurrentMessage = timerClass.Name + " Failed.";
                    _notification.Status = ProgressNotificationStatus.Failed;
                }
            }

            SaveSettings(settings);
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
                    var settings = new JobSetting
                                       {
                                           Enable = true,
                                           TypeName = timer.GetType().ToString(),
                                           Name = timerProviderLocal.Name,
                                           Interval = timerProviderLocal.DefaultInterval,
                                           LastExecution = DateTime.MinValue
                                       };

                    SaveSettings(settings);
                }
            }
        }


    }
}