// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Threading;
using AutoMoq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class JobProviderTest : TestBase
    {
        [Test]
        public void Run_Jobs_Updates_Last_Execution()
        {
            IList<IJob> fakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            //Act
            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.RunScheduled();

            //Assert
            var settings = timerProvider.All();
            Assert.IsNotEmpty(settings);
            Assert.AreNotEqual(DateTime.MinValue, settings[0].LastExecution);
            settings[0].Success.Should().BeTrue();
        }

        [Test]
        public void Run_Jobs_Updates_Last_Execution_Mark_as_unsuccesful()
        {

            IList<IJob> fakeJobs = new List<IJob> { new BrokenJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            //Act
            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.RunScheduled();

            //Assert
            var settings = timerProvider.All();
            Assert.IsNotEmpty(settings);
            Assert.AreNotEqual(DateTime.MinValue, settings[0].LastExecution);
            Assert.IsFalse(settings[0].Success);
            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_job_again()
        {
            IList<IJob> fakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            var firstRun = timerProvider.RunScheduled();
            var secondRun = timerProvider.RunScheduled();

            firstRun.Should().BeTrue();
            secondRun.Should().BeTrue();
        }

        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_async_job_again()
        {
            IList<IJob> fakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            var firstRun = timerProvider.QueueJob(typeof(FakeJob));
            Thread.Sleep(2000);
            var secondRun = timerProvider.QueueJob(typeof(FakeJob));

            firstRun.Should().BeTrue();
            secondRun.Should().BeTrue();
            JobProvider.Queue.Should().BeEmpty();
        }

        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void no_concurent_jobs()
        {
            IList<IJob> fakeJobs = new List<IJob> { new SlowJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.QueueJob(typeof(SlowJob), 1);
            timerProvider.QueueJob(typeof(SlowJob), 2);
            timerProvider.QueueJob(typeof(SlowJob), 3);


            Thread.Sleep(10000);
            JobProvider.Queue.Should().BeEmpty();
            //Asserts are done in ExceptionVerification
        }


        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_broken_async_job_again()
        {
            IList<IJob> fakeJobs = new List<IJob> { new BrokenJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            var firstRun = timerProvider.QueueJob(typeof(BrokenJob));
            Thread.Sleep(2000);
            var secondRun = timerProvider.QueueJob(typeof(BrokenJob));

            firstRun.Should().BeTrue();
            secondRun.Should().BeTrue();
            Thread.Sleep(2000);
            JobProvider.Queue.Should().BeEmpty();
            ExceptionVerification.ExcpectedErrors(2);
        }

        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_two_jobs_at_the_same_time()
        {
            IList<IJob> fakeJobs = new List<IJob> { new SlowJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

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

            firstRun.Should().BeTrue();
            Assert.IsFalse(secondRun);

        }


        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_queue_jobs_at_the_same_time()
        {
            var slowJob = new SlowJob();

            IList<IJob> fakeJobs = new List<IJob> { slowJob };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();

            var thread1 = new Thread(() => timerProvider.QueueJob(typeof(SlowJob)));
            var thread2 = new Thread(() => timerProvider.QueueJob(typeof(SlowJob)));

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            Thread.Sleep(5000);

            Assert.AreEqual(1, slowJob.ExexutionCount);
            JobProvider.Queue.Should().BeEmpty();

        }

        [Test]
        public void Init_Jobs()
        {
            var fakeTimer = new FakeJob();
            IList<IJob> fakeJobs = new List<IJob> { fakeTimer };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();

            var timers = timerProvider.All();


            //Assert

            timers.Should().HaveCount(1);
            timers[0].Interval.Should().Be(fakeTimer.DefaultInterval);
            timers[0].Name.Should().Be(fakeTimer.Name);
            timers[0].TypeName.Should().Be(fakeTimer.GetType().ToString());
            timers[0].LastExecution.Should().HaveYear(2000);
            timers[0].Enable.Should().BeTrue();
        }

        [Test]
        public void Init_Timers_only_registers_once()
        {
            var repo = MockLib.GetEmptyDatabase();

            for (int i = 0; i < 2; i++)
            {
                var fakeTimer = new FakeJob();
                IList<IJob> fakeJobs = new List<IJob> { fakeTimer };
                var mocker = new AutoMoqer();

                mocker.SetConstant(repo);
                mocker.SetConstant(fakeJobs);

                var timerProvider = mocker.Resolve<JobProvider>();
                timerProvider.Initialize();
            }

            var mocker2 = new AutoMoqer();

            mocker2.SetConstant(repo);
            var assertTimerProvider = mocker2.Resolve<JobProvider>();

            var timers = assertTimerProvider.All();


            //Assert
            timers.Should().HaveCount(1);
            timers[0].Enable.Should().BeTrue();
        }

        [Test]
        public void Init_Timers_sets_interval_0_to_disabled()
        {
            var repo = MockLib.GetEmptyDatabase();

            for (int i = 0; i < 2; i++)
            {
                var disabledJob = new DisabledJob();
                IList<IJob> fakeJobs = new List<IJob> { disabledJob };
                var mocker = new AutoMoqer();

                mocker.SetConstant(repo);
                mocker.SetConstant(fakeJobs);

                var timerProvider = mocker.Resolve<JobProvider>();
                timerProvider.Initialize();
            }

            var mocker2 = new AutoMoqer();

            mocker2.SetConstant(repo);
            var assertTimerProvider = mocker2.Resolve<JobProvider>();

            var timers = assertTimerProvider.All();


            //Assert
            timers.Should().HaveCount(1);
            Assert.IsFalse(timers[0].Enable);
        }

        [Test]
        public void Get_Next_Execution_Time()
        {
            IList<IJob> fakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            //Act
            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.RunScheduled();
            var next = timerProvider.NextScheduledRun(typeof(FakeJob));

            //Assert
            var settings = timerProvider.All();
            Assert.IsNotEmpty(settings);
            Assert.AreEqual(next, settings[0].LastExecution.AddMinutes(15));
        }

        [Test]
        public void Disabled_isnt_run_by_scheduler()
        {
            var repo = MockLib.GetEmptyDatabase();


            var disabledJob = new DisabledJob();
            IList<IJob> fakeJobs = new List<IJob> { disabledJob };
            var mocker = new AutoMoqer();

            mocker.SetConstant(repo);
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();

            timerProvider.RunScheduled();

            Thread.Sleep(1000);


            //Assert
            Assert.AreEqual(0, disabledJob.ExexutionCount);
        }

        [Test]
        public void SingleId_do_not_update_last_execution()
        {
            IList<IJob> fakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            //Act
            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.QueueJob(typeof(FakeJob), 10);
            Thread.Sleep(1000);

            //Assert
            var settings = timerProvider.All();
            settings.Should().NotBeEmpty();
            settings[0].LastExecution.Should().HaveYear(2000);
            JobProvider.Queue.Should().BeEmpty();
        }

        [Test]
        public void SingleId_do_not_set_success()
        {
            IList<IJob> fakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            //Act
            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.QueueJob(typeof(FakeJob), 10);
            Thread.Sleep(1000);

            //Assert
            var settings = timerProvider.All();
            Assert.IsNotEmpty(settings);
            Assert.IsFalse(settings[0].Success);
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

        }
    }

    public class DisabledJob : IJob
    {
        public string Name
        {
            get { return "DisabledJob"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public int ExexutionCount { get; set; }

        public void Start(ProgressNotification notification, int targetId)
        {
            ExexutionCount++;
        }
    }

    public class BrokenJob : IJob
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
            throw new ApplicationException("Broken job is broken");
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

        public int ExexutionCount { get; set; }

        public void Start(ProgressNotification notification, int targetId)
        {
            Console.WriteLine("Starting Job");
            Thread.Sleep(2000);
            ExexutionCount++;
            Console.WriteLine("Finishing Job");
        }
    }
}