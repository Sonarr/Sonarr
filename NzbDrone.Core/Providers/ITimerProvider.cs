using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Providers
{
    public interface ITimerProvider
    {
        void ResetTimer();
        void StartTimer();
        void StopTimer();
    }
}
