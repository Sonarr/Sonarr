using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Host;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ServiceFactoryFixture : TestBase<ServiceFactory>
    {
        [Test]
        public void event_handlers_should_be_unique()
        {
            var container = MainAppContainerBuilder.BuildContainer(new StartupContext());
            container.Resolve<IAppFolderFactory>().Register();

            Mocker.SetConstant(container);

            var handlers = Subject.BuildAll<IHandle<ApplicationShutdownRequested>>()
                                  .Select(c => c.GetType().FullName);

            handlers.Should().OnlyHaveUniqueItems();
        }
    }
}