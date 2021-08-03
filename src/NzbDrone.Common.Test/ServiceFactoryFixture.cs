using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Datastore;
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
            container.Register<IMainDatabase>(new MainDatabase(null));
            container.Register<ILogDatabase>(new LogDatabase(null));
            container.Resolve<IAppFolderFactory>().Register();

            Mocker.SetConstant(container);

            var handlers = Subject.BuildAll<IHandle<ApplicationStartedEvent>>()
                                  .Select(c => c.GetType().FullName);

            handlers.Should().OnlyHaveUniqueItems();
        }
    }
}
