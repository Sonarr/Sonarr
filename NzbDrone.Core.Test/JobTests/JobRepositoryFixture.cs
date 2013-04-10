using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class JobRepositoryFixture : DbTest<JobRepository, JobDefinition>
    {
        FakeJob _fakeJob;
        DisabledJob _disabledJob;

        [SetUp]
        public void Setup()
        {
            _fakeJob = new FakeJob();
            _disabledJob = new DisabledJob();
        }


        private void Initialize()
        {
            Subject.Handle(new ApplicationStartedEvent());
        }


        [Test]
        public void Init_should_add_defintaions()
        {
            IEnumerable<IJob> baseFakeJobs = new List<IJob> { _fakeJob };
            Mocker.SetConstant(baseFakeJobs);


            Initialize();

            Storage.All().Should().HaveCount(1);
            StoredModel.Interval.Should().Be((Int32)_fakeJob.DefaultInterval.TotalMinutes);
            StoredModel.Name.Should().Be(_fakeJob.Name);
            StoredModel.Type.Should().Be(_fakeJob.GetType().ToString());
            StoredModel.LastExecution.Should().HaveYear(DateTime.Now.Year);
            StoredModel.LastExecution.Should().HaveMonth(DateTime.Now.Month);
            StoredModel.LastExecution.Should().HaveDay(DateTime.Today.Day);
            StoredModel.Enable.Should().BeTrue();
        }

        [Test]
        public void inti_should_removed_jobs_that_no_longer_exist()
        {
            IEnumerable<IJob> fakeJobs = new List<IJob> { _fakeJob };
            Mocker.SetConstant(fakeJobs);

            var deletedJob = Builder<JobDefinition>.CreateNew()
                .With(c => c.Id = 0)
                .Build();

            Db.Insert(deletedJob);

            //Make sure deleted job is stored
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(c => c.Type == deletedJob.Type);

            Initialize();

            //Make sure init has cleaned up the deleted job
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().NotContain(c => c.Type == deletedJob.Type);
        }

        [Test]
        public void init_should_removed_jobs_that_no_longer_exist_even_with_same_name()
        {
            IEnumerable<IJob> fakeJobs = new List<IJob> { _fakeJob };
            Mocker.SetConstant(fakeJobs);

            var deletedJob = Builder<JobDefinition>.CreateNew()
                .With(c => c.Name = _fakeJob.Name)
                .With(c => c.Id = 0)
                .Build();

            Db.Insert(deletedJob);

            //Make sure deleted job is stored
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(c => c.Type == deletedJob.Type);

            Initialize();

            //Make sure init has cleaned up the deleted job
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().NotContain(c => c.Type == deletedJob.Type);
        }

        [Test]
        public void init_should_update_existing_job()
        {

            var oldJob = Builder<JobDefinition>.CreateNew()
                .With(c => c.Id = 0)
                .With(c => c.Name = "OldName")
                .With(c => c.Type = typeof(FakeJob).ToString())
                .With(c => c.Interval = 0)
                .With(c => c.Enable = true)
                .With(c => c.Success = true)
                .With(c => c.LastExecution = DateTime.Now.AddDays(-7).Date)
                .Build();

            Storage.Insert(oldJob);

            var newJob = new FakeJob();

            IEnumerable<IJob> fakeJobs = new List<IJob> { newJob };
            Mocker.SetConstant(fakeJobs);

            Initialize();


            AllStoredModels.Should().HaveCount(1);
            StoredModel.Type.Should().Be(newJob.GetType().FullName);
            StoredModel.Name.Should().Be(newJob.Name);
            StoredModel.Interval.Should().Be((int)newJob.DefaultInterval.TotalMinutes);
            StoredModel.Enable.Should().Be(true);
            StoredModel.Success.Should().Be(oldJob.Success);
            StoredModel.LastExecution.Should().Be(oldJob.LastExecution);
        }

        [Test]
        public void jobs_with_zero_interval_are_registered_as_disabled()
        {
            IEnumerable<IJob> fakeJobs = new List<IJob> { _disabledJob };
            Mocker.SetConstant(fakeJobs);

            Initialize();


            Storage.All().Should().HaveCount(1);
            Storage.All().First().Enable.Should().BeFalse();
        }


        [Test]
        public void pending_job_should_get_jobs_that_have_matured()
        {
            var oldJob = Builder<JobDefinition>.CreateNew()
             .With(c => c.Id = 0)
             .With(c => c.Interval = 1)
             .With(c => c.Enable = true)
             .With(c => c.Success = true)
             .With(c => c.LastExecution = DateTime.Now.AddMinutes(-5))
             .Build();


            Storage.Insert(oldJob);


            Subject.GetPendingJobs().Should().HaveCount(1);
        }


        [Test]
        public void pending_job_should_not_get_jobs_that_havent_matured()
        {
            var recent = Builder<JobDefinition>.CreateNew()
             .With(c => c.Id = 0)
             .With(c => c.Interval = 60)
             .With(c => c.Enable = true)
             .With(c => c.Success = true)
             .With(c => c.LastExecution = DateTime.Now.AddMinutes(-5))
             .Build();


            Storage.Insert(recent);


            Subject.GetPendingJobs().Should().BeEmpty();
        }

        /*        [Test]
                public void disabled_jobs_arent_run_by_scheduler()
                {
                    IEnumerable<IJob> BaseFakeJobs = new List<IJob> { disabledJob };
                    Mocker.SetConstant(BaseFakeJobs);

                    var jobProvider = Mocker.Resolve<JobController>();
                    jobProvider.QueueScheduled();

                    WaitForQueue();


                    disabledJob.ExecutionCount.Should().Be(0);
                }*/

    }
}
