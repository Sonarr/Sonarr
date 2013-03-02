using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using FluentAssertions;
using NCrunch.Framework;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.ExternalNotification;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Metadata;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [Ignore]
    [TestFixture]
    [ExclusivelyUses("REAL_LOG_FILE")]
    [Serial]
    class CentralDispatchFixture : CoreTest
    {
        static readonly Assembly NzbDroneCore = Assembly.Load("NzbDrone.Core");

        readonly IList<string> indexers = NzbDroneCore.GetTypes().Where(t => t.IsSubclassOf(typeof(IndexerBase))).Select(c => c.ToString()).ToList();
        readonly IList<string> jobs = NzbDroneCore.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IJob))).Select(c => c.ToString()).ToList();
        readonly IList<Type> extNotifications = NzbDroneCore.GetTypes().Where(t => t.IsSubclassOf(typeof(ExternalNotificationBase))).ToList();
        readonly IList<Type> metadata = NzbDroneCore.GetTypes().Where(t => t.IsSubclassOf(typeof(MetadataBase))).ToList();



        private readonly IContainer kernel;

        public CentralDispatchFixture()
        {
            InitLogging();
        }

        [Test]
        public void InitAppTest()
        {
            kernel.Should().NotBeNull();
        }

        [Test]
        public void Resolve_all_providers()
        {
            var providers = NzbDroneCore.GetTypes().Where(t => t.Name.EndsWith("Provider")).ToList();

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
            kernel.Resolve<IIndexerService>().All().Select(c => c.Type).Should().BeEquivalentTo(indexers);
        }


        [Test]
        public void quality_profile_initialized()
        {
            kernel.Resolve<QualityProfileService>().All().Should().HaveCount(2);
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