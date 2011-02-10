using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public interface ITimerProvider
    {
        void ResetRssSyncTimer();
        void StartRssSyncTimer();
        void StopRssSyncTimer();
        void SetRssSyncTimer(int minutes);
        TimeSpan RssSyncTimeLeft();
        DateTime NextRssSyncTime();
        void StartMinuteTimer();
        void StopMinuteTimer();
    }
}
