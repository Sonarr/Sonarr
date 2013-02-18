using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using NCrunch.Framework;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Metadata;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    [ExclusivelyUses("REAL_LOG_FILE")]
    [Serial]
    class CentralDispatchFixture : CoreTest
    {
        readonly IList<string> indexers = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(IndexerBase))).Select(c => c.ToString()).ToList();
        readonly IList<string> jobs = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IJob))).Select(c => c.ToString()).ToList();
        readonly IList<Type> extNotifications = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ExternalNotificationBase))).ToList();
        readonly IList<Type> metadata = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(MetadataBase))).ToList();

        private readonly IContainer kernel;

        public CentralDispatchFixture()
        {
            if (EnvironmentProvider.IsMono)
            {
                throw new IgnoreException("SqlCe is not supported");
            }

            InitLogging();
            var dispatch = new CentralDispatch();
            kernel = dispatch.BuildContainer();

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
                kernel.Resolve(provider).Should().NotBeNull();
            }
        }

        [Test]
        public void All_jobs_should_be_registered()
        {
            var registeredJobs = kernel.Resolve<IEnumerable<IJob>>();

            jobs.Should().NotBeEmpty();

            registeredJobs.Should().HaveSameCount(jobs);
        }

        [Test]
        public void All_indexers_should_be_registered()
        {
            var registeredIndexers = kernel.Resolve<IEnumerable<IndexerBase>>();

            indexers.Should().NotBeEmpty();

            registeredIndexers.Should().HaveSameCount(indexers);
        }

        [Test]
        public void All_externalNotifiers_should_be_registered()
        {
            var externalNotificationBases = kernel.Resolve<IEnumerable<ExternalNotificationBase>>();

            extNotifications.Should().NotBeEmpty();

            externalNotificationBases.Should().HaveSameCount(extNotifications);
        }

        [Test]
        public void All_metadata_clients_should_be_registered()
        {
            var metadataBases = kernel.Resolve<IEnumerable<MetadataBase>>();

            metadata.Should().NotBeEmpty();

            metadataBases.Should().HaveSameCount(metadata);
        }

        [Test]
        public void jobs_are_initialized()
        {
            kernel.Resolve<IJobRepository>().All().Should().HaveSameCount(jobs);
        }

        [Test]
        public void indexers_are_initialized()
        {
            kernel.Resolve<IndexerProvider>().All().Select(c => c.IndexProviderType).Should().BeEquivalentTo(indexers);
        }

        [Test]
        public void externalNotifiers_are_initialized()
        {
            kernel.Resolve<ExternalNotificationProvider>().All().Should().HaveSameCount(extNotifications);
        }

        [Test]
        public void metadata_clients_are_initialized()
        {
            kernel.Resolve<MetadataProvider>().All().Should().HaveSameCount(metadata);
        }

        [Test]
        public void quality_profile_initialized()
        {
            kernel.Resolve<QualityProvider>().All().Should().HaveCount(2);
        }

        [Test]
        public void JobProvider_should_be_singletone()
        {
            var first = kernel.Resolve<JobController>();
            var second = kernel.Resolve<JobController>();

            first.Should().BeSameAs(second);
        }
    }
}