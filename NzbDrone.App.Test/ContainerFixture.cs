using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Test.Common;
using FluentAssertions;

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
            MainAppContainerBuilder.BuildContainer().Resolve<IEnumerable<IIndexerBase>>().Should().NotBeEmpty();
        }

        [Test]
        public void should_be_able_to_resolve_downlodclients()
        {
            MainAppContainerBuilder.BuildContainer().Resolve<IEnumerable<IDownloadClient>>().Should().NotBeEmpty();
        }
    }
}