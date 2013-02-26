// ReSharper disable RedundantUsingDirective

using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using NCrunch.Framework;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class JobRepositoryFixture : ObjectDbTest<JobRepository, JobDefinition>
    {
        FakeJob _fakeJob;
        DisabledJob _disabledJob;

        [SetUp]
        public void Setup()
        {
            _fakeJob = new FakeJob();
            _disabledJob = new DisabledJob();
        }


        [Test]
        public void Init_should_add_defintaions()
        {
            IEnumerable<IJob> baseFakeJobs = new List<IJob> { _fakeJob };
            Mocker.SetConstant(baseFakeJobs);


            Subject.Init();

            Storage.All().Should().HaveCount(1);
            Storage.All().ToList()[0].Interval.Should().Be((Int32)_fakeJob.DefaultInterval.TotalMinutes);
            Storage.All().ToList()[0].Name.Should().Be(_fakeJob.Name);
            Storage.All().ToList()[0].TypeName.Should().Be(_fakeJob.GetType().ToString());
            Storage.All().ToList()[0].LastExecution.Should().HaveYear(DateTime.Now.Year);
            Storage.All().ToList()[0].LastExecution.Should().HaveMonth(DateTime.Now.Month);
            Storage.All().ToList()[0].LastExecution.Should().HaveDay(DateTime.Today.Day);
            Storage.All().ToList()[0].Enable.Should().BeTrue();
        }

        [Test]
        public void inti_should_removed_jobs_that_no_longer_exist()
        {
            IEnumerable<IJob> fakeJobs = new List<IJob> { _fakeJob };
            Mocker.SetConstant(fakeJobs);

            Subject.Init();

            var deletedJob = Builder<JobDefinition>.CreateNew()
                                .With(c => c.Id = 0)
                                .Build();

            Db.Insert(deletedJob);
            Subject.Init();

            var registeredJobs = Storage.All();
            registeredJobs.Should().HaveCount(1);
            registeredJobs.Should().NotContain(c => c.TypeName == deletedJob.TypeName);
        }

        [Test]
        public void inti_should_removed_jobs_that_no_longer_exist_even_with_same_name()
        {
            IEnumerable<IJob> fakeJobs = new List<IJob> { _fakeJob };
            Mocker.SetConstant(fakeJobs);

            Subject.Init();

            var deletedJob = Builder<JobDefinition>.CreateNew()
                .With(c => c.Name = _fakeJob.Name)
                .With(c => c.Id = 0)
                .Build();


            Db.Insert(deletedJob);
            Subject.Init();



            var registeredJobs = Storage.All();
            registeredJobs.Should().HaveCount(1);
            registeredJobs.Should().NotContain(c => c.TypeName == deletedJob.TypeName);
        }

        [Test]
        public void init_should_update_existing_job()
        {

            var oldJob = Builder<JobDefinition>.CreateNew()
                .With(c => c.Id = 0)
                .With(c => c.Name = "OldName")
                .With(c => c.TypeName = typeof(FakeJob).ToString())
                .With(c => c.Interval = 0)
                .With(c => c.Enable = true)
                .With(c => c.Success = true)
                .With(c => c.LastExecution = DateTime.Now.AddDays(-7).Date)
                .Build();

            Storage.Insert(oldJob);

            var newJob = new FakeJob();

            IEnumerable<IJob> fakeJobs = new List<IJob> { newJob };
            Mocker.SetConstant(fakeJobs);

            Subject.Init();


            var registeredJobs = Storage.All();
            registeredJobs.Should().HaveCount(1);
            registeredJobs.First().TypeName.Should().Be(newJob.GetType().FullName);
            registeredJobs.First().Name.Should().Be(newJob.Name);
            registeredJobs.First().Interval.Should().Be((int)newJob.DefaultInterval.TotalMinutes);

            registeredJobs.First().Enable.Should().Be(true);
            registeredJobs.First().Success.Should().Be(oldJob.Success);
            registeredJobs.First().LastExecution.Should().Be(oldJob.LastExecution);
        }

        [Test]
        public void jobs_with_zero_interval_are_registered_as_disabled()
        {
            IEnumerable<IJob> fakeJobs = new List<IJob> { _disabledJob };
            Mocker.SetConstant(fakeJobs);

            Subject.Init();


            Storage.All().Should().HaveCount(1);
            Storage.All().First().Enable.Should().BeFalse();
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
