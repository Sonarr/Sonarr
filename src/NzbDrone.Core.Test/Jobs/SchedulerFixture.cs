using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Test.Common;
using Timer = System.Timers.Timer;

namespace NzbDrone.Core.Test.Jobs
{
    [TestFixture]
    public class SchedulerFixture : TestBase<Scheduler>
    {
        private static Timer GetTimer(Scheduler scheduler)
        {
            return (Timer)typeof(Scheduler).GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(scheduler);
        }

        [Test]
        public void should_keep_running_after_previous_scheduler_shuts_down()
        {
            var restarted = new Scheduler(Mocker.GetMock<ITaskManager>().Object,
                                          Mocker.GetMock<IManageCommandQueue>().Object,
                                          TestLogger);

            Subject.Handle(new ApplicationStartedEvent());
            Subject.Handle(new ApplicationShutdownRequested());
            restarted.Handle(new ApplicationStartedEvent());

            try
            {
                GetTimer(restarted).Should().NotBeSameAs(GetTimer(Subject));
                GetTimer(Subject).Enabled.Should().BeFalse();
                GetTimer(restarted).Enabled.Should().BeTrue();
            }
            finally
            {
                restarted.Handle(new ApplicationShutdownRequested());
            }
        }
    }
}
