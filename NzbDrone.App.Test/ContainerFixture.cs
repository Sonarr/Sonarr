using System;
using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Test.Common;
using FluentAssertions;
using TinyIoC;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class ContainerFixture : TestBase
    {
        [Test]
        public void should_be_able_to_resolve_event_handlers()
        {
            MainAppContainerBuilder.BuildContainer().Resolve<IEnumerable<IProcessMessage>>().Should().NotBeEmpty();
        }

        [Test]
        public void should_be_able_to_resolve_indexers()
        {
            MainAppContainerBuilder.BuildContainer().Resolve<IEnumerable<IIndexer>>().Should().NotBeEmpty();
        }

        [Test]
        public void should_be_able_to_resolve_downlodclients()
        {
            MainAppContainerBuilder.BuildContainer().Resolve<IEnumerable<IDownloadClient>>().Should().NotBeEmpty();
        }

        [Test]
        public void container_should_inject_itself()
        {
            var factory = MainAppContainerBuilder.BuildContainer().Resolve<IServiceFactory>();

            factory.Build<IIndexerService>().Should().NotBeNull();
        }

        [Test]
        public void should_resolve_command_executor_by_name()
        {
            var genericExecutor = typeof(IExecute<>).MakeGenericType(typeof(RssSyncCommand));

            var executor = MainAppContainerBuilder.BuildContainer().Resolve(genericExecutor);

            executor.Should().NotBeNull();
            executor.Should().BeAssignableTo<IExecute<RssSyncCommand>>();
        }
    }
}