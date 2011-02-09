using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using NLog;
using NzbDrone.Core.Model.Notification;
using Timer = System.Threading.Timer;

namespace NzbDrone.Core.Providers
{
    public class TimerProvider : ITimerProvider
    {
        private ProgressNotification _seriesSyncNotification;
        private IRssSyncProvider _rssSyncProvider;
        private Thread _rssSyncThread;
        private System.Timers.Timer _rssSyncTimer;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private DateTime _rssSyncNextInterval;

        public TimerProvider(IRssSyncProvider rssSyncProvider)
        {
            _rssSyncProvider = rssSyncProvider;
        }

        #region ITimerProvider Members

        public void ResetRssSyncTimer()
        {
            double interval = _rssSyncTimer.Interval;
            _rssSyncTimer .Interval= interval;
        }
        public void StartRssSyncTimer()
        {
            if (_rssSyncTimer.Interval < 900000) //If Timer is less than 15 minutes, throw an error!
            {
                Logger.Error("RSS Sync Frequency is invalid, please set the interval first");
                throw new InvalidOperationException("RSS Sync Frequency Invalid");
            }

            _rssSyncTimer.Elapsed +=new ElapsedEventHandler(RunSync);
            _rssSyncTimer.Start();
            _rssSyncNextInterval = DateTime.Now.AddMilliseconds(_rssSyncTimer.Interval);
        }

        public void StopRssSyncTimer()
        {
            _rssSyncTimer.Stop();
        }

        public void SetRssSyncTimer(int minutes)
        {
            long ms = minutes*60*1000;
            _rssSyncTimer.Interval = ms;
        }

        public TimeSpan RssSyncTimeLeft()
        {
            return _rssSyncNextInterval.Subtract(DateTime.Now);
        }

        public DateTime NextRssSyncTime()
        {
            return _rssSyncNextInterval;
        }

        #endregion

        private void RunSync(object obj, ElapsedEventArgs args)
        {
            DateTime.Now.AddMilliseconds(_rssSyncTimer.Interval);
            _rssSyncProvider.Begin();
        }
    }
}
