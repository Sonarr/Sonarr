using System;
using System.Linq;
using System.Threading;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Test.ProviderTests.JobProviderTests
{

    public class FakeJob : IJob
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public virtual int DefaultInterval
        {
            get { return 15; }
        }

        public int ExecutionCount { get; private set; }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            ExecutionCount++;
            Console.WriteLine("Begin " + Name);
            Start();
            Console.WriteLine("End " + Name);
        }

        protected virtual void Start()
        {
        }
    }

    public class DisabledJob : FakeJob
    {
        public override int DefaultInterval
        {
            get { return 0; }
        }
    }

    public class BrokenJob : FakeJob
    {
        protected override void Start()
        {
            throw new ApplicationException("Broken job is broken");
        }
    }

    public class SlowJob : FakeJob
    {
        protected override void Start()
        {
            Thread.Sleep(1000);
        }
    }
}
