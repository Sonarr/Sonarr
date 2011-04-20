using System;
using System.Collections.Generic;
using System.Threading;
using AutoMoq;
using MbUnit.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Jobs;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class JobProviderTest
    {
        [Test]
        public void Run_Jobs()
        {

            IEnumerable<IJob> fakeTimers = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.SetConstant(fakeTimers);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.RunScheduled();

        }


        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_job_again()
        {
            IEnumerable<IJob> fakeTimers = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.SetConstant(fakeTimers);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            var firstRun = timerProvider.RunScheduled();
            var secondRun = timerProvider.RunScheduled();

            Assert.IsTrue(firstRun);
            Assert.IsTrue(secondRun);

        }

        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_async_job_again()
        {
            IEnumerable<IJob> fakeTimers = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.SetConstant(fakeTimers);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            var firstRun = timerProvider.BeginExecute(typeof(FakeJob));
            Thread.Sleep(2000);
            var secondRun = timerProvider.BeginExecute(typeof(FakeJob));

            Assert.IsTrue(firstRun);
            Assert.IsTrue(secondRun);

        }


        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_two_jobs_at_the_same_time()
        {
            IEnumerable<IJob> fakeTimers = new List<IJob> { new SlowJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.SetConstant(fakeTimers);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();

            bool firstRun = false;
            bool secondRun = false;

            var thread1 = new Thread(() => firstRun = timerProvider.RunScheduled());
            thread1.Start();
            Thread.Sleep(1000);
            var thread2 = new Thread(() => secondRun = timerProvider.RunScheduled());
            thread2.Start();

            thread1.Join();
            thread2.Join();

            Assert.IsTrue(firstRun);
            Assert.IsFalse(secondRun);

        }

        [Test]
        public void Init_Jobs()
        {
            var fakeTimer = new FakeJob();
            IEnumerable<IJob> fakeTimers = new List<IJob> { fakeTimer };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.SetConstant(fakeTimers);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();

            var timers = timerProvider.All();


            //Assert
            Assert.Count(1, timers);
            Assert.AreEqual(fakeTimer.DefaultInterval, timers[0].Interval);
            Assert.AreEqual(fakeTimer.Name, timers[0].Name);
            Assert.AreEqual(fakeTimer.GetType().ToString(), timers[0].TypeName);
            Assert.AreEqual(DateTime.MinValue, timers[0].LastExecution);
            Assert.IsTrue(timers[0].Enable);


        }

        [Test]
        public void Init_Timers_only_registers_once()
        {
            var repo = MockLib.GetEmptyRepository();

            for (int i = 0; i < 2; i++)
            {
                var fakeTimer = new FakeJob();
                IEnumerable<IJob> fakeTimers = new List<IJob> { fakeTimer };
                var mocker = new AutoMoqer();

                mocker.SetConstant(repo);
                mocker.SetConstant(fakeTimers);

                var timerProvider = mocker.Resolve<JobProvider>();
                timerProvider.Initialize();
            }

            var mocker2 = new AutoMoqer();

            mocker2.SetConstant(repo);
            var assertTimerProvider = mocker2.Resolve<JobProvider>();

            var timers = assertTimerProvider.All();


            //Assert
            Assert.Count(1, timers);
        }



    }

    public class FakeJob : IJob
    {
        public string Name
        {
            get { return "FakeJob"; }
        }

        public int DefaultInterval
        {
            get { return 15; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            throw new NotImplementedException();
        }
    }

    public class SlowJob : IJob
    {
        public string Name
        {
            get { return "FakeJob"; }
        }

        public int DefaultInterval
        {
            get { return 15; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            Thread.Sleep(10000);
        }
    }
}