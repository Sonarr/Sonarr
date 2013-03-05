using System.Timers;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.Jobs
{
    public class JobTimer : IInitializable
    {
        private readonly IJobController _jobController;
        private readonly Timer _timer;

        public JobTimer(IJobController jobController)
        {
            _jobController = jobController;
            _timer = new Timer();

        }

        public void Init()
        {
            _timer.Interval = 1000 * 30;
            _timer.Elapsed += (o, args) => _jobController.EnqueueScheduled();
            _timer.Start();
        }

    }
}