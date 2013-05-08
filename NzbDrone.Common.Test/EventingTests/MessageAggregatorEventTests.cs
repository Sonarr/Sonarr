using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EventingTests
{
    [TestFixture]
    public class MessageAggregatorEventTests : TestBase<MessageAggregator>
    {
        private Mock<IHandle<EventA>> HandlerA1;
        private Mock<IHandle<EventA>> HandlerA2;

        private Mock<IHandle<EventB>> HandlerB1;
        private Mock<IHandle<EventB>> HandlerB2;


        [SetUp]
        public void Setup()
        {
            HandlerA1 = new Mock<IHandle<EventA>>();
            HandlerA2 = new Mock<IHandle<EventA>>();
            HandlerB1 = new Mock<IHandle<EventB>>();
            HandlerB2 = new Mock<IHandle<EventB>>();

            Mocker.GetMock<IServiceFactory>()
                  .Setup(c => c.BuildAll<IHandle<EventA>>())
                  .Returns(new List<IHandle<EventA>> { HandlerA1.Object, HandlerA2.Object });

            Mocker.GetMock<IServiceFactory>()
                  .Setup(c => c.BuildAll<IHandle<EventB>>())
                  .Returns(new List<IHandle<EventB>> { HandlerB1.Object, HandlerB2.Object });

        }

        [Test]
        public void should_publish_event_to_handlers()
        {
            var eventA = new EventA();

            Subject.PublishEvent(eventA);

            HandlerA1.Verify(c => c.Handle(eventA), Times.Once());
            HandlerA2.Verify(c => c.Handle(eventA), Times.Once());
        }

        [Test]
        public void should_not_publish_to_incompatible_handlers()
        {
            var eventA = new EventA();


            Subject.PublishEvent(eventA);

            HandlerA1.Verify(c => c.Handle(eventA), Times.Once());
            HandlerA2.Verify(c => c.Handle(eventA), Times.Once());

            HandlerB1.Verify(c => c.Handle(It.IsAny<EventB>()), Times.Never());
            HandlerB2.Verify(c => c.Handle(It.IsAny<EventB>()), Times.Never());
        }


        [Test]
        public void broken_handler_should_not_effect_others_handler()
        {
            var eventA = new EventA();


            HandlerA1.Setup(c => c.Handle(It.IsAny<EventA>()))
                       .Throws(new NotImplementedException());

            Subject.PublishEvent(eventA);

            HandlerA1.Verify(c => c.Handle(eventA), Times.Once());
            HandlerA2.Verify(c => c.Handle(eventA), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

    }


    public class EventA : IEvent
    {

    }

    public class EventB : IEvent
    {

    }

}