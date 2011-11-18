using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NLog;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Test.Framework;
using Ninject;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    class CentralDispatchFixture : CoreTest
    {
        readonly IList<Type> indexers = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(IndexerBase))).ToList();
        readonly IList<Type> jobs = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IJob))).ToList();

        private IKernel kernel;

        public CentralDispatchFixture()
        {
            InitLogging();
            kernel = new CentralDispatch().Kernel;
            WebTimer.Stop();
        }

        [Test]
        public void InitAppTest()
        {
            kernel.Should().NotBeNull();
        }

        [Test]
        public void Resolve_all_providers()
        {
            var providers = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.Name.EndsWith("Provider")).ToList();

            providers.Should().NotBeEmpty();

            foreach (var provider in providers)
            {
                Console.WriteLine("Resolving " + provider.Name);
                kernel.Get(provider).Should().NotBeNull();
            }
        }


        [Test]
        public void All_jobs_should_be_registered()
        {
            //Assert

            var registeredJobs = kernel.GetAll<IJob>();

            jobs.Should().NotBeEmpty();

            registeredJobs.Should().HaveSameCount(jobs);
        }


        [Test]
        public void All_indexers_should_be_registered()
        {
            //Assert

            var registeredIndexers = kernel.GetAll<IndexerBase>();

            indexers.Should().NotBeEmpty();

            registeredIndexers.Should().HaveSameCount(indexers);
        }


        [Test]
        public void jobs_are_initialized()
        {
            kernel.Get<JobProvider>().All().Should().HaveSameCount(jobs);
        }

        [Test]
        public void indexers_are_initialized()
        {
            kernel.Get<IndexerProvider>().All().Should().HaveSameCount(indexers);
        }

        [Test]
        public void quality_profile_initialized()
        {
            kernel.Get<QualityProvider>().All().Should().HaveCount(2);
        }

        [Test]
        public void JobProvider_should_be_singletone()
        {
            var first = kernel.Get<JobProvider>();
            var second = kernel.Get<JobProvider>();

            first.Should().BeSameAs(second);
        }

    }
}
