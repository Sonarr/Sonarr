using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NCrunch.Framework;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    [ExclusivelyUses("JOB_PROVIDER")]
    public class JobControllerFixture : CoreTest<JobController>
    {

        FakeJob _fakeJob;
        SlowJob _slowJob;
        BrokenJob _brokenJob;
        DisabledJob _disabledJob;


        private JobDefinition _updatedJob = null;

        [SetUp]
        public void Setup()
        {
            _fakeJob = new FakeJob();
            _slowJob = new SlowJob();
            _brokenJob = new BrokenJob();
            _disabledJob = new DisabledJob();
            _updatedJob = null;

            IEnumerable<IJob> jobs = new List<IJob> { _fakeJob, _slowJob, _brokenJob, _disabledJob };

            Mocker.SetConstant(jobs);

            Mocker.GetMock<IJobRepository>()
                  .Setup(c => c.Update(It.IsAny<JobDefinition>()))
                  .Callback<JobDefinition>(c => _updatedJob = c);


            Mocker.GetMock<IJobRepository>()
                    .Setup(c => c.GetDefinition(It.IsAny<Type>()))
                    .Returns(new JobDefinition());
        }


        private void GivenPendingJob(IList<JobDefinition> jobDefinition)
        {
            Mocker.GetMock<IJobRepository>().Setup(c => c.GetPendingJobs()).Returns(jobDefinition);
        }

        [TearDown]
        public void TearDown()
        {
            Subject.Queue.Should().BeEmpty();
        }

        private void WaitForQueue()
        {
            Console.WriteLine("Waiting for queue to clear.");
            var stopWatch = Mocker.Resolve<JobController>().StopWatch;

            while (stopWatch.IsRunning)
            {
                Thread.Sleep(10);
            }
        }

        [Test]
        public void running_scheduled_jobs_should_updates_last_execution_time()
        {
            GivenPendingJob(new List<JobDefinition> { new JobDefinition { TypeName = _fakeJob.GetType().FullName } });

            Subject.QueueScheduled();
            WaitForQueue();

            _updatedJob.LastExecution.Should().BeWithin(TimeSpan.FromSeconds(10));
            _updatedJob.LastExecution.Should().BeWithin(TimeSpan.FromSeconds(10));
            _fakeJob.ExecutionCount.Should().Be(1);
        }

        [Test]
        public void failing_scheduled_job_should_mark_job_as_failed()
        {
            GivenPendingJob(new List<JobDefinition> { new JobDefinition { TypeName = _brokenJob.GetType().FullName } });

            Subject.QueueScheduled();
            WaitForQueue();

            _updatedJob.LastExecution.Should().BeWithin(TimeSpan.FromSeconds(10));
            _updatedJob.Success.Should().BeFalse();
            _brokenJob.ExecutionCount.Should().Be(1);
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void can_run_async_job_again()
        {
            Subject.QueueJob(typeof(FakeJob));
            WaitForQueue();
            Subject.QueueJob(typeof(FakeJob));
            WaitForQueue();

            Subject.Queue.Should().BeEmpty();
            _fakeJob.ExecutionCount.Should().Be(2);
        }

        [Test]
        public void no_concurent_jobs()
        {
            Subject.QueueJob(typeof(SlowJob), 1);
            Subject.QueueJob(typeof(SlowJob), 2);
            Subject.QueueJob(typeof(SlowJob), 3);

            WaitForQueue();

            Subject.Queue.Should().BeEmpty();
            _slowJob.ExecutionCount.Should().Be(3);
            ExceptionVerification.AssertNoUnexcpectedLogs();
        }


        [Test]
        public void can_run_broken_job_again()
        {
            Subject.QueueJob(typeof(BrokenJob));
            WaitForQueue();

            Subject.QueueJob(typeof(BrokenJob));
            WaitForQueue();


            _brokenJob.ExecutionCount.Should().Be(2);
            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void schedule_hit_should_be_ignored_if_queue_is_running()
        {
            Subject.QueueJob(typeof(SlowJob));
            Subject.QueueScheduled();
            WaitForQueue();

            _slowJob.ExecutionCount.Should().Be(1);
            _fakeJob.ExecutionCount.Should().Be(0);
        }


        [Test]
        public void can_queue_jobs_at_the_same_time()
        {

            Subject.QueueJob(typeof(SlowJob));
            var thread1 = new Thread(() => Subject.QueueJob(typeof(FakeJob)));
            var thread2 = new Thread(() => Subject.QueueJob(typeof(FakeJob)));

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            WaitForQueue();

            _fakeJob.ExecutionCount.Should().Be(1);
            _slowJob.ExecutionCount.Should().Be(1);
            Subject.Queue.Should().BeEmpty();
        }


        [Test]
        public void job_with_specific_target_should_not_update_status()
        {
            Subject.QueueJob(typeof(FakeJob), 10);

            WaitForQueue();

            Mocker.GetMock<IJobRepository>().Verify(c=>c.Update(It.IsAny<JobDefinition>()),Times.Never());
            _updatedJob.Should().BeNull();
        }

       

        [Test]
        public void Item_added_to_queue_while_scheduler_runs_should_be_executed()
        {
            GivenPendingJob(new List<JobDefinition> { new JobDefinition { TypeName = _slowJob.GetType().FullName } });

            var jobThread = new Thread(Subject.QueueScheduled);
            jobThread.Start();

            Thread.Sleep(200);

            Subject.QueueJob(typeof(DisabledJob), 12);

            WaitForQueue();

            _slowJob.ExecutionCount.Should().Be(1);
            _disabledJob.ExecutionCount.Should().Be(1);
        }

        [Test]
        public void trygin_to_queue_unregistered_job_should_fail()
        {
            Subject.QueueJob(typeof(UpdateInfoJob));
            WaitForQueue();
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void scheduled_job_should_have_scheduler_as_source()
        {
            GivenPendingJob(new List<JobDefinition> { new JobDefinition { TypeName = _slowJob.GetType().FullName }, new JobDefinition { TypeName = _slowJob.GetType().FullName } });
            Subject.QueueScheduled();

            Subject.Queue.Should().OnlyContain(c => c.Source == JobQueueItem.JobSourceType.Scheduler);

            WaitForQueue();
        }
    }
}
