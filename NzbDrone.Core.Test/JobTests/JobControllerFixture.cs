using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NCrunch.Framework;
using NUnit.Framework;
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
        SlowJob2 _slowJob2;
        BrokenJob _brokenJob;
        DisabledJob _disabledJob;


        private JobDefinition _updatedJob = null;

        [SetUp]
        public void Setup()
        {
            _fakeJob = new FakeJob();
            _slowJob = new SlowJob();
            _slowJob2 = new SlowJob2();
            _brokenJob = new BrokenJob();
            _disabledJob = new DisabledJob();
            _updatedJob = null;

            IEnumerable<IJob> jobs = new List<IJob> { _fakeJob, _slowJob, _slowJob2, _brokenJob, _disabledJob };

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
            var queue = Mocker.Resolve<JobController>().Queue;

            while (Subject.IsProcessing)
            {
                Thread.Sleep(100);
            }
        }

        [Test]
        public void running_scheduled_jobs_should_updates_last_execution_time()
        {
            GivenPendingJob(new List<JobDefinition> { new JobDefinition { TypeName = _fakeJob.GetType().FullName } });

            Subject.EnqueueScheduled();
            WaitForQueue();

            _updatedJob.LastExecution.Should().BeWithin(TimeSpan.FromSeconds(10));
            _updatedJob.LastExecution.Should().BeWithin(TimeSpan.FromSeconds(10));
            _fakeJob.ExecutionCount.Should().Be(1);
        }

        [Test]
        public void failing_scheduled_job_should_mark_job_as_failed()
        {
            GivenPendingJob(new List<JobDefinition> { new JobDefinition { TypeName = _brokenJob.GetType().FullName } });

            Subject.EnqueueScheduled();
            WaitForQueue();

            _updatedJob.LastExecution.Should().BeWithin(TimeSpan.FromSeconds(10));
            _updatedJob.Success.Should().BeFalse();
            _brokenJob.ExecutionCount.Should().Be(1);
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void can_run_job_again()
        {
            Subject.Enqueue(typeof(FakeJob));
            WaitForQueue();
            Subject.Enqueue(typeof(FakeJob));
            WaitForQueue();

            Subject.Queue.Should().BeEmpty();
            _fakeJob.ExecutionCount.Should().Be(2);
        }

        [Test]
        public void should_ignore_job_with_same_arg()
        {
            Subject.Enqueue(typeof(SlowJob2), 1);
            Subject.Enqueue(typeof(FakeJob), 1);
            Subject.Enqueue(typeof(FakeJob), 1);

            WaitForQueue();

            Subject.Queue.Should().BeEmpty();
            _fakeJob.ExecutionCount.Should().Be(1);
            ExceptionVerification.AssertNoUnexcpectedLogs();
        }


        [Test]
        public void can_run_broken_job_again()
        {
            Subject.Enqueue(typeof(BrokenJob));
            WaitForQueue();

            Subject.Enqueue(typeof(BrokenJob));
            WaitForQueue();


            _brokenJob.ExecutionCount.Should().Be(2);
            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void schedule_hit_should_be_ignored_if_queue_is_running()
        {
            Subject.Enqueue(typeof(SlowJob));
            Subject.EnqueueScheduled();
            WaitForQueue();

            _slowJob.ExecutionCount.Should().Be(1);
            _fakeJob.ExecutionCount.Should().Be(0);
        }


        [Test]
        public void can_queue_jobs_at_the_same_time()
        {

            Subject.Enqueue(typeof(SlowJob));
            var thread1 = new Thread(() => Subject.Enqueue(typeof(FakeJob)));
            var thread2 = new Thread(() => Subject.Enqueue(typeof(FakeJob)));

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
            Subject.Enqueue(typeof(FakeJob), 10);

            WaitForQueue();

            Mocker.GetMock<IJobRepository>().Verify(c => c.Update(It.IsAny<JobDefinition>()), Times.Never());
            _updatedJob.Should().BeNull();
        }



        [Test]
        public void Item_added_to_queue_while_scheduler_runs_should_be_executed()
        {
            GivenPendingJob(new List<JobDefinition> { new JobDefinition { TypeName = _slowJob.GetType().FullName } });

            var jobThread = new Thread(Subject.EnqueueScheduled);
            jobThread.Start();

            Thread.Sleep(200);

            Subject.Enqueue(typeof(DisabledJob), 12);

            WaitForQueue();

            _slowJob.ExecutionCount.Should().Be(1);
            _disabledJob.ExecutionCount.Should().Be(1);
        }

        [Test]
        public void trygin_to_queue_unregistered_job_should_fail()
        {
            Subject.Enqueue(typeof(UpdateInfoJob));
            WaitForQueue();
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void scheduled_job_should_have_scheduler_as_source()
        {
            GivenPendingJob(new List<JobDefinition> { new JobDefinition { TypeName = _slowJob.GetType().FullName }, new JobDefinition { TypeName = _slowJob2.GetType().FullName } });
            Subject.EnqueueScheduled();

            Subject.Queue.Should().OnlyContain(c => c.Source == JobQueueItem.JobSourceType.Scheduler);

            WaitForQueue();
        }
    }
}
