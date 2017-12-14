using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NzbDrone.Common.Timeline;

namespace NzbDrone.Core.TransferProviders
{
    public class TransferTask
    {
        public ITimelineContext Timeline { get; private set; }

        // Async task that is completed once the Transfer has finished or ended in failure. (Do not rely on ProgressReporter for finished detection)
        public Task CompletionTask { get; private set; }

        public TransferTask(ITimelineContext timeline, Task completionTask)
        {
            Timeline = timeline;
            CompletionTask = completionTask.ContinueWith(t => Timeline.FinishProgress());
        }
    }
}
