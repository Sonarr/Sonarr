// ReSharper disable RedundantUsingDirective

using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using AutoMoq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.JobProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class JobProviderTest : CoreTest
    {
        [Test]
        public void Run_Jobs_Updates_Last_Execution()
        {
            IList<IJob> BaseFakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

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

            IList<IJob> BaseFakeJobs = new List<IJob> { new BrokenJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

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
            var BaseFakeJob = new FakeJob();
            var mocker = new AutoMoqer();

            IList<IJob> BaseFakeJobs = new List<IJob> { BaseFakeJob };
            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            var timerProvider = mocker.Resolve<JobProvider>();
            timerProvider.Initialize();
            timerProvider.QueueScheduled();
            Thread.Sleep(500);
            timerProvider.QueueScheduled();
            Thread.Sleep(500);

            BaseFakeJob.ExecutionCount.Should().Be(1);
        }

        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_async_job_again()
        {
            var BaseFakeJob = new FakeJob();
            var mocker = new AutoMoqer();

            IList<IJob> BaseFakeJobs = new List<IJob> { BaseFakeJob };
            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            var jobProvider = mocker.Resolve<JobProvider>();
            jobProvider.Initialize();
            jobProvider.QueueJob(typeof(FakeJob));
            Thread.Sleep(1000);
            jobProvider.QueueJob(typeof(FakeJob));
            Thread.Sleep(2000);
            jobProvider.Queue.Should().BeEmpty();
            BaseFakeJob.ExecutionCount.Should().Be(2);
        }

        [Test]
        public void no_concurent_jobs()
        {
            IList<IJob> BaseFakeJobs = new List<IJob> { new SlowJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            var jobProvider = mocker.Resolve<JobProvider>();
            jobProvider.Initialize();
            jobProvider.QueueJob(typeof(SlowJob), 1);
            jobProvider.QueueJob(typeof(SlowJob), 2);
            jobProvider.QueueJob(typeof(SlowJob), 3);


            Thread.Sleep(5000);
            jobProvider.Queue.Should().BeEmpty();
            //Asserts are done in ExceptionVerification
        }


        [Test]
        //This test will confirm that the concurrency checks are rest
        //after execution so the job can successfully run.
        public void can_run_broken_async_job_again()
        {
            var brokenJob = new BrokenJob();
            IList<IJob> BaseFakeJobs = new List<IJob> { brokenJob };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            var jobProvider = mocker.Resolve<JobProvider>();
            jobProvider.Initialize();
            jobProvider.QueueJob(typeof(BrokenJob));
            Thread.Sleep(2000);
            jobProvider.QueueJob(typeof(BrokenJob));


            Thread.Sleep(2000);
            jobProvider.Queue.Should().BeEmpty();
            brokenJob.ExecutionCount.Should().Be(2);
            ExceptionVerification.ExcpectedErrors(2);
        }

        [Test]
        public void can_run_two_jobs_at_the_same_time()
        {
            WithRealDb();

            var fakeJob = new FakeJob();
            IList<IJob> fakeJobs = new List<IJob> { fakeJob };

            Mocker.SetConstant(fakeJobs);

            var jobProvider = Mocker.Resolve<JobProvider>();
            jobProvider.Initialize();


            jobProvider.QueueScheduled();
            jobProvider.QueueScheduled();
           

            Thread.Sleep(2000);
           
            fakeJob.ExecutionCount.Should().Be(1);
        }


        [Test]
        public void can_queue_jobs_at_the_same_time()
        {
            var slowJob = new SlowJob();
            var BaseFakeJob = new FakeJob();

            IList<IJob> BaseFakeJobs = new List<IJob> { slowJob, BaseFakeJob };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            var jobProvider = mocker.Resolve<JobProvider>();
            jobProvider.Initialize();


            jobProvider.QueueJob(typeof(SlowJob));
            var thread1 = new Thread(() => jobProvider.QueueJob(typeof(FakeJob)));
            var thread2 = new Thread(() => jobProvider.QueueJob(typeof(FakeJob)));

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            Thread.Sleep(5000);

            BaseFakeJob.ExecutionCount.Should().Be(1);
            jobProvider.Queue.Should().BeEmpty();
        }

        [Test]
        public void Init_Jobs()
        {
            var fakeTimer = new FakeJob();
            IList<IJob> BaseFakeJobs = new List<IJob> { fakeTimer };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

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
                IList<IJob> BaseFakeJobs = new List<IJob> { fakeTimer };
                var mocker = new AutoMoqer();

                mocker.SetConstant(repo);
                mocker.SetConstant(BaseFakeJobs);

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
                IList<IJob> BaseFakeJobs = new List<IJob> { disabledJob };
                var mocker = new AutoMoqer();

                mocker.SetConstant(repo);
                mocker.SetConstant(BaseFakeJobs);

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
            IList<IJob> BaseFakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

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
            IList<IJob> BaseFakeJobs = new List<IJob> { disabledJob };
            var mocker = new AutoMoqer();

            mocker.SetConstant(repo);
            mocker.SetConstant(BaseFakeJobs);

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
            IList<IJob> BaseFakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            //Act
            var jobProvider = mocker.Resolve<JobProvider>();
            jobProvider.Initialize();
            jobProvider.QueueJob(typeof(FakeJob), 10);
            Thread.Sleep(1000);

            //Assert
            var settings = jobProvider.All();
            settings.Should().NotBeEmpty();
            settings[0].LastExecution.Should().HaveYear(2000);
            jobProvider.Queue.Should().BeEmpty();
        }

        [Test]
        public void SingleId_do_not_set_success()
        {
            IList<IJob> BaseFakeJobs = new List<IJob> { new FakeJob() };
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            //Act
            var jobProvider = mocker.Resolve<JobProvider>();
            jobProvider.Initialize();
            jobProvider.QueueJob(typeof(FakeJob), 10);
            Thread.Sleep(1000);

            //Assert
            var settings = jobProvider.All();
            Assert.IsNotEmpty(settings);
            Assert.IsFalse(settings[0].Success);
        }


        [Test]
        public void existing_queue_should_start_queue_if_not_running()
        {
            var mocker = new AutoMoqer();

            var BaseFakeJob = new FakeJob();
            IList<IJob> BaseFakeJobs = new List<IJob> { BaseFakeJob };


            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            var fakeQueueItem = new JobQueueItem
                                    {
                                        JobType = BaseFakeJob.GetType(),
                                        TargetId = 12,
                                        SecondaryTargetId = 0
                                    };

            //Act
            var jobProvider = mocker.Resolve<JobProvider>();
            jobProvider.Initialize();
            jobProvider.Queue.Add(fakeQueueItem);
            jobProvider.QueueJob(BaseFakeJob.GetType(), 12);
            Thread.Sleep(1000);

            //Assert
            BaseFakeJob.ExecutionCount.Should().Be(1);
        }


        [Test]
        public void Item_added_to_queue_while_scheduler_runs_is_executed()
        {
            var mocker = new AutoMoqer();

            var slowJob = new SlowJob();
            var disabledJob = new DisabledJob();
            IList<IJob> BaseFakeJobs = new List<IJob> { slowJob, disabledJob };

            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.SetConstant(BaseFakeJobs);

            var jobProvider = mocker.Resolve<JobProvider>();

            jobProvider.Initialize();

            var _jobThread = new Thread(jobProvider.QueueScheduled);
            _jobThread.Start();

            Thread.Sleep(200);

            jobProvider.QueueJob(typeof(DisabledJob), 12);

            Thread.Sleep(3000);

            //Assert
            jobProvider.Queue.Should().BeEmpty();
            slowJob.ExecutionCount.Should().Be(1);
            disabledJob.ExecutionCount.Should().Be(1);
        }

        [Test]
        public void trygin_to_queue_unregistered_job_should_fail()
        {
            WithRealDb();

            IList<IJob> BaseFakeJobs = new List<IJob> { new SlowJob(), new DisabledJob() };

            Mocker.SetConstant(BaseFakeJobs);

            var jobProvider = Mocker.Resolve<JobProvider>();

            jobProvider.Initialize();
            jobProvider.QueueJob(typeof(string));

            Thread.Sleep(1000);
            ExceptionVerification.ExcpectedErrors(1);
        }
    }


}