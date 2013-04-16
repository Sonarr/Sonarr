using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Common.Eventing;
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
            ContainerBuilder.Instance.Resolve<IEnumerable<IHandle>>().Should().NotBeEmpty();
        }

        [Test]
        public void should_be_able_to_resolve_indexers()
        {
            ContainerBuilder.Instance.Resolve<IEnumerable<IIndexerBase>>().Should().NotBeEmpty();
        }

        [Test]
        public void should_be_able_to_resolve_downlodclients()
        {
            ContainerBuilder.Instance.Resolve<IEnumerable<IDownloadClient>>().Should().NotBeEmpty();
        }
    }
}