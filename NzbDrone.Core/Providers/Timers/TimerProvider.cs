using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Timers
{
    public class TimerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;
        private readonly IEnumerable<ITimer> _timerJobs;

        private static readonly object ExecutionLock = new object();
        private static bool _isRunning;

        public TimerProvider(IRepository repository, IEnumerable<ITimer> timerJobs)
        {
            _repository = repository;
            _timerJobs = timerJobs;
        }

        public TimerProvider() { }

        public virtual List<TimerSetting> All()
        {
            return _repository.All<TimerSetting>().ToList();
        }

        public virtual void SaveSettings(TimerSetting settings)
        {
            if (settings.Id == 0)
            {
                Logger.Debug("Adding timer settings for {0}", settings.Name);
                _repository.Add(settings);
            }
            else
            {
                Logger.Debug("Updating timer settings for {0}", settings.Name);
                _repository.Update(settings);
            }
        }

        public virtual void Run()
        {
            lock (ExecutionLock)
            {
                if (_isRunning)
                {
                    Logger.Info("Another instance of timer is already running. Ignoring request.");
                    return;
                }
                _isRunning = true;
            }

            Logger.Trace("Getting list of timers needing to be executed");

            var pendingTimers = All().Where(
                                                     t => t.Enable &&
                                                     (DateTime.Now - t.LastExecution) > TimeSpan.FromMinutes(t.Interval)
                );

            foreach (var pendingTimer in pendingTimers)
            {
                Logger.Info("Attempting to start timer [{0}]. Last executing {1}", pendingTimer.Name, pendingTimer.LastExecution);
                var timerClass = _timerJobs.Where(t => t.GetType().ToString() == pendingTimer.TypeName).FirstOrDefault();
                ForceExecute(timerClass.GetType());
            }
        }

        public void ForceExecute(Type timerType)
        {
            var timerClass = _timerJobs.Where(t => t.GetType() == timerType).FirstOrDefault();
            if (timerClass == null)
            {
                Logger.Error("Unable to locate implantation for [{0}]. Make sure its properly registered.", timerType.ToString());
                return;
            }

            try
            {
                var sw = Stopwatch.StartNew();
                timerClass.Start();
                sw.Stop();
                Logger.Info("timer [{0}] finished executing successfully. Duration {1}", timerClass.Name, sw.Elapsed.ToString());
            }
            catch (Exception e)
            {
                Logger.Error("An error has occurred while executing timer job " + timerClass.Name, e);
            }
        }

        public virtual void Initialize()
        {
            Logger.Info("Initializing timer jobs. Count {0}", _timerJobs.Count());
            var currentTimer = All();

            foreach (var timer in _timerJobs)
            {
                var timerProviderLocal = timer;
                if (!currentTimer.Exists(c => c.TypeName == timerProviderLocal.GetType().ToString()))
                {
                    var settings = new TimerSetting()
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