using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ServiceFactoryFixture : TestBase<ServiceFactory>
    {
        [SetUp]
        public void setup()
        {
            Mocker.SetConstant(MainAppContainerBuilder.BuildContainer());
        }


        [Test]
        public void event_handlers_should_be_unique()
        {
            var handlers = Subject.BuildAll<IHandle<ApplicationShutdownRequested>>()
                                  .Select(c => c.GetType().FullName);

            handlers.Should().OnlyHaveUniqueItems();
        }
    }
}