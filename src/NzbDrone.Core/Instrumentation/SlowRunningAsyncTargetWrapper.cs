using System.Threading;
using NLog.Common;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace NzbDrone.Core.Instrumentation
{
    [Target("SlowRunningAsyncTargetWrapper", IsWrapper = true)]
    public class SlowRunningAsyncTargetWrapper : AsyncTargetWrapper
    {
        private int _state; // 0 = idle, 1 = timer active, 2 = timer active + possibly more work

        public SlowRunningAsyncTargetWrapper(Target wrappedTarget)
            : base(wrappedTarget)
        {
        }

        protected override void StopLazyWriterThread()
        {
            if (Interlocked.Exchange(ref _state, 0) > 0)
            {
                base.StopLazyWriterThread();
            }
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            base.Write(logEvent);

            if (Interlocked.Exchange(ref _state, 2) <= 0)
            { // Timer was idle. Starting.
                base.StartLazyWriterTimer();
            }
        }

        protected override void StartLazyWriterTimer()
        {
            // Is executed when the background task has finished processing the queue. (also executed by base.InitializeTarget once)

            if (Interlocked.Decrement(ref _state) == 1)
            { // There might be more work. Restart timer.
                base.StartLazyWriterTimer();
            }
        }
    }
}
