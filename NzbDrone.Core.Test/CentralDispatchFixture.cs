// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
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
    class CentralDispatchFixture : CoreTest
    {
        readonly IList<Type> indexers = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(IndexerBase))).ToList();
        readonly IList<Type> jobs = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IJob))).ToList();
        readonly IList<Type> extNotifications = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ExternalNotificationBase))).ToList();
        readonly IList<Type> metadata = typeof(CentralDispatch).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(MetadataBase))).ToList();

        private IContainer kernel;

        public CentralDispatchFixture()
        {
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
            //Assert

            var registeredJobs = kernel.Resolve<IEnumerable<IJob>>();

            jobs.Should().NotBeEmpty();

            registeredJobs.Should().HaveSameCount(jobs);
        }

        [Test]
        public void All_indexers_should_be_registered()
        {
            //Assert

            var registeredIndexers = kernel.Resolve<IEnumerable<IndexerBase>>();

            indexers.Should().NotBeEmpty();

            registeredIndexers.Should().HaveSameCount(indexers);
        }

        [Test]
        public void All_externalNotifiers_should_be_registered()
        {
            //Assert

            var externalNotificationBases = kernel.Resolve<IEnumerable<ExternalNotificationBase>>();

            extNotifications.Should().NotBeEmpty();

            externalNotificationBases.Should().HaveSameCount(extNotifications);
        }

        [Test]
        public void All_metadata_clients_should_be_registered()
        {
            //Assert

            var metadataBases = kernel.Resolve<IEnumerable<MetadataBase>>();

            metadata.Should().NotBeEmpty();

            metadataBases.Should().HaveSameCount(metadata);
        }

        [Test]
        public void jobs_are_initialized()
        {
            kernel.Resolve<JobProvider>().All().Should().HaveSameCount(jobs);
        }

        [Test]
        public void indexers_are_initialized()
        {
            kernel.Resolve<IndexerProvider>().All().Should().HaveSameCount(indexers);
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
            var first = kernel.Resolve<JobProvider>();
            var second = kernel.Resolve<JobProvider>();

            first.Should().BeSameAs(second);
        }
    }
}