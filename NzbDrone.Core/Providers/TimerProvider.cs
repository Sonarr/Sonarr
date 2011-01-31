using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using NzbDrone.Core.Model.Notification;
using Timer = System.Threading.Timer;

namespace NzbDrone.Core.Providers
{
    public class TimerProvider : ITimerProvider
    {
        private ProgressNotification _seriesSyncNotification;
        private Thread _seriesSyncThread;
        private System.Timers.Timer _rssSyncTimer;

        #region ITimerProvider Members

        public void ResetTimer()
        {
            throw new NotImplementedException();
        }
        public void StartTimer()
        {
            throw new NotImplementedException();
        }

        public void StopTimer()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
