using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Host;
using NzbDrone.Test.Common;

namespace NzbDrone.Mono.Test
{
    [TestFixture]
    public class ServiceFactoryFixture : TestBase<ServiceFactory>
    {
        [Test]
        public void event_handlers_should_be_unique()
        {
            MonoOnly();
    
            Mocker.SetConstant(MainAppContainerBuilder.BuildContainer(new StartupContext()));

            var handlers = Subject.BuildAll<IHandle<ApplicationShutdownRequested>>()
                                  .Select(c => c.GetType().FullName);

            handlers.Should().OnlyHaveUniqueItems();
        }
    }
}