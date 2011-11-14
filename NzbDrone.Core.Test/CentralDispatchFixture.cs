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

        private CentralDispatch centralDispatch;

        [SetUp]
        public void Setup()
        {
            centralDispatch = new CentralDispatch();
        }

        [Test]
        public void InitAppTest()
        {
            centralDispatch.Kernel.Should().NotBeNull();
        }

        [Test]
        public void Resolve_all_providers()
        {
            var providers = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.Name.EndsWith("Provider")).ToList();

            providers.Should().NotBeEmpty();

            foreach (var provider in providers)
            {
                Console.WriteLine("Resolving " + provider.Name);
                centralDispatch.Kernel.Get(provider).Should().NotBeNull();
            }
        }


        [Test]
        public void All_jobs_should_be_registered()
        {
            //Assert

            var registeredJobs = centralDispatch.Kernel.GetAll<IJob>();

            jobs.Should().NotBeEmpty();

            registeredJobs.Should().HaveSameCount(jobs);
        }


        [Test]
        public void All_indexers_should_be_registered()
        {
            //Assert

            var registeredIndexers = centralDispatch.Kernel.GetAll<IndexerBase>();

            indexers.Should().NotBeEmpty();

            registeredIndexers.Should().HaveSameCount(indexers);
        }


        [Test]
        public void jobs_are_initialized()
        {
            centralDispatch.Kernel.Get<JobProvider>().All().Should().HaveSameCount(jobs);
        }

        [Test]
        public void indexers_are_initialized()
        {
            centralDispatch.Kernel.Get<IndexerProvider>().All().Should().HaveSameCount(indexers);
        }

        [Test]
        public void quality_profile_initialized()
        {
            centralDispatch.Kernel.Get<QualityProvider>().All().Should().HaveCount(2);
        }

        [Test]
        public void JobProvider_should_be_singletone()
        {
            var first = centralDispatch.Kernel.Get<JobProvider>();
            var second = centralDispatch.Kernel.Get<JobProvider>();

            first.Should().BeSameAs(second);
        }

        [TearDown]
        public void TearDownBase()
        {
            WebTimer.Stop();

        }
    }
}
