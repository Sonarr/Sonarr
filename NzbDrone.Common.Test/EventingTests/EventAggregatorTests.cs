using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EventingTests
{
    [TestFixture]
    public class ServiceNameFixture : TestBase
    {
        [Test]
        public void should_publish_event_to_handlers()
        {
            var eventA = new EventA();
            

            var intHandler = new Mock<IHandle<EventA>>();
            var aggregator = new MessageAggregator(TestLogger, () => new List<IProcessMessage> { intHandler.Object });
            aggregator.Publish(eventA);

            intHandler.Verify(c => c.Handle(eventA), Times.Once());
        }

        [Test]
        public void should_publish_to_more_than_one_handler()
        {
            var eventA = new EventA();

            var intHandler1 = new Mock<IHandle<EventA>>();
            var intHandler2 = new Mock<IHandle<EventA>>();
            var aggregator = new MessageAggregator(TestLogger, () => new List<IProcessMessage> { intHandler1.Object, intHandler2.Object });
            aggregator.Publish(eventA);

            intHandler1.Verify(c => c.Handle(eventA), Times.Once());
            intHandler2.Verify(c => c.Handle(eventA), Times.Once());
        }

        [Test]
        public void should_not_publish_to_incompatible_handlers()
        {
            var eventA = new EventA();

            var aHandler = new Mock<IHandle<EventA>>();
            var bHandler = new Mock<IHandle<EventB>>();
            var aggregator = new MessageAggregator(TestLogger, () => new List<IProcessMessage> { aHandler.Object, bHandler.Object });

            aggregator.Publish(eventA);

            aHandler.Verify(c => c.Handle(eventA), Times.Once());
            bHandler.Verify(c => c.Handle(It.IsAny<EventB>()), Times.Never());
        }

    }


    public class EventA:IEvent
    {
        
    }

    public class EventB : IEvent
    {

    }

}