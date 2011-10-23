// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Threading;
using AutoMoq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class JobProviderTest : TestBase
    {
        [TestFixtureSetUp]
        public override void Setup()
        {
            base.Setup();
            JobProvider.Queue.Clear();
        }

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
            timerProvider.QueueScheduled();

            //Assert
            var settings = timerProvider.All();

            Assert.AreNotEqual(DateTime.MinValue, settings[0].LastExecution);
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
            timerProvider.QueueScheduled();

            Thread.Sleep(1000);

            //Assert
            var settings = timerProvider.All();
            Assert.AreNotEqual(DateTime.MinValue, settings[0].LastExecution);
            settings[0].Success.Should().BeFalse();
            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        public void scheduler_skips_jobs_that_arent_mature_yet()
        {
            var fakeJob = new FakeJob();
            var mocker = new AutoMoqer();

            IList<IJob> fakeJobs = new List<IJob> { fakeJob };
            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.QueueScheduled();
            Thread.Sleep(500);
            timerProvider.QueueScheduled();
            Thread.Sleep(500);

            fakeJob.ExecutionCount.Should().Be(1);
        }

        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_async_job_again()
        {
            var fakeJob = new FakeJob();
            var mocker = new AutoMoqer();

            IList<IJob> fakeJobs = new List<IJob> { fakeJob };
            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.QueueJob(typeof(FakeJob));
            Thread.Sleep(1000);
            timerProvider.QueueJob(typeof(FakeJob));
            Thread.Sleep(2000);
            JobProvider.Queue.Should().BeEmpty();
            fakeJob.ExecutionCount.Should().Be(2);
        }

        [Test]
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


            Thread.Sleep(5000);
            JobProvider.Queue.Should().BeEmpty();
            //Asserts are done in ExceptionVerification
        }


        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_broken_async_job_again()
        {
            var brokenJob = new BrokenJob();
            IList<IJob> fakeJobs = new List<IJob> { brokenJob };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.QueueJob(typeof(BrokenJob));
            Thread.Sleep(2000);
            timerProvider.QueueJob(typeof(BrokenJob));


            Thread.Sleep(2000);
            JobProvider.Queue.Should().BeEmpty();
            brokenJob.ExecutionCount.Should().Be(2);
            ExceptionVerification.ExcpectedErrors(2);
        }

        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_two_jobs_at_the_same_time()
        {
            var slowJob = new SlowJob();
            IList<IJob> fakeJobs = new List<IJob> { slowJob };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();


            var thread1 = new Thread(() => timerProvider.QueueScheduled());
            thread1.Start();
            Thread.Sleep(1000);
            var thread2 = new Thread(() => timerProvider.QueueScheduled());
            thread2.Start();

            thread1.Join();
            thread2.Join();


            slowJob.ExecutionCount = 2;

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

            Assert.AreEqual(1, slowJob.ExecutionCount);
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
            timerProvider.QueueScheduled();
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

            timerProvider.QueueScheduled();

            Thread.Sleep(1000);


            //Assert
            Assert.AreEqual(0, disabledJob.ExecutionCount);
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


        [Test]
        public void existing_queue_should_start_queue_if_not_running()
        {
            var mocker = new AutoMoqer();

            var fakeJob = new FakeJob();
            IList<IJob> fakeJobs = new List<IJob> { fakeJob };


            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            var fakeQueueItem = new JobQueueItem
                                    {
                                        JobType = fakeJob.GetType(),
                                        TargetId = 12,
                                        SecondaryTargetId = 0
                                    };

            //Act
            var jobProvider = mocker.Resolve<JobProvider>();
            jobProvider.Initialize();
            JobProvider.Queue.Add(fakeQueueItem);
            jobProvider.QueueJob(fakeJob.GetType(), 12);
            Thread.Sleep(1000);

            //Assert
            fakeJob.ExecutionCount.Should().Be(1);
        }


        [Test]
        public void Item_added_to_queue_while_scheduler_runs_is_executed()
        {
            var mocker = new AutoMoqer();

            var slowJob = new SlowJob();
            var disabledJob = new DisabledJob();
            IList<IJob> fakeJobs = new List<IJob> { slowJob, disabledJob };

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(fakeJobs);

            mocker.Resolve<JobProvider>().Initialize();

            var _jobThread = new Thread(() => mocker.Resolve<JobProvider>().QueueScheduled());
            _jobThread.Start();

            Thread.Sleep(200);

            mocker.Resolve<JobProvider>().QueueJob(typeof(DisabledJob), 12);

            Thread.Sleep(3000);

            //Assert
            JobProvider.Queue.Should().BeEmpty();
            slowJob.ExecutionCount.Should().Be(1);
            disabledJob.ExecutionCount.Should().Be(1);
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

        public int ExecutionCount { get; set; }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            ExecutionCount++;
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

        public int ExecutionCount { get; set; }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            ExecutionCount++;
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

        public int ExecutionCount { get; set; }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            ExecutionCount++;
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

        public int ExecutionCount { get; set; }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            Console.WriteLine("Starting Job");
            Thread.Sleep(1000);
            ExecutionCount++;
            Console.WriteLine("Finishing Job");
        }
    }
}