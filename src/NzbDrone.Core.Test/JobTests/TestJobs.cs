/*
using System;
using System.Linq;
using System.Threading;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Test.JobTests
{

    public class FakeJob : IJob
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public virtual TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromMinutes(15); }
        }

        public int ExecutionCount { get; private set; }

        public void Start(ProgressNotification notification, dynamic options)
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
        public override TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
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

    public class SlowJob2 : FakeJob
    {
        protected override void Start()
        {
            Thread.Sleep(1000);
        }
    }
}
*/
