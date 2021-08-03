using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Messaging.Events
{
    [TestFixture]
    public class EventAggregatorFixture : TestBase<EventAggregator>
    {
        private Mock<IHandle<EventA>> _handlerA1;
        private Mock<IHandle<EventA>> _handlerA2;

        private Mock<IHandle<EventB>> _handlerB1;
        private Mock<IHandle<EventB>> _handlerB2;

        private Mock<IHandleAsync<EventA>> _asyncHandlerA1;

        [SetUp]
        public void Setup()
        {
            _handlerA1 = new Mock<IHandle<EventA>>();
            _handlerA2 = new Mock<IHandle<EventA>>();
            _handlerB1 = new Mock<IHandle<EventB>>();
            _handlerB2 = new Mock<IHandle<EventB>>();

            _asyncHandlerA1 = new Mock<IHandleAsync<EventA>>();

            Mocker.GetMock<IServiceFactory>()
                  .Setup(c => c.BuildAll<IHandle<EventA>>())
                  .Returns(new List<IHandle<EventA>> { _handlerA1.Object, _handlerA2.Object });

            Mocker.GetMock<IServiceFactory>()
                  .Setup(c => c.BuildAll<IHandle<EventB>>())
                  .Returns(new List<IHandle<EventB>> { _handlerB1.Object, _handlerB2.Object });
        }

        [Test]
        public void should_publish_event_to_handlers()
        {
            var eventA = new EventA();

            Subject.PublishEvent(eventA);

            _handlerA1.Verify(c => c.Handle(eventA), Times.Once());
            _handlerA2.Verify(c => c.Handle(eventA), Times.Once());
        }

        [Test]
        public void should_not_publish_to_incompatible_handlers()
        {
            var eventA = new EventA();

            Subject.PublishEvent(eventA);

            _handlerA1.Verify(c => c.Handle(eventA), Times.Once());
            _handlerA2.Verify(c => c.Handle(eventA), Times.Once());

            _handlerB1.Verify(c => c.Handle(It.IsAny<EventB>()), Times.Never());
            _handlerB2.Verify(c => c.Handle(It.IsAny<EventB>()), Times.Never());
        }

        [Test]
        public void broken_handler_should_not_effect_others_handler()
        {
            var eventA = new EventA();

            _handlerA1.Setup(c => c.Handle(It.IsAny<EventA>()))
                       .Throws(new NotImplementedException());

            Subject.PublishEvent(eventA);

            _handlerA1.Verify(c => c.Handle(eventA), Times.Once());
            _handlerA2.Verify(c => c.Handle(eventA), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

       /* [Test]
        public void should_queue_multiple_async_events()
        {
            var eventA = new EventA();



            var handlers = new List<IHandleAsync<EventA>>
                {
                    AsyncHandlerA1.Object,
                    AsyncHandlerA1.Object,
                    AsyncHandlerA1.Object,
                    AsyncHandlerA1.Object,
                    AsyncHandlerA1.Object,
                    AsyncHandlerA1.Object,
                    AsyncHandlerA1.Object,
                };

            Mocker.GetMock<IServiceFactory>()
          .Setup(c => c.BuildAll<IHandle<EventA>>())
          .Returns(new List<IHandle<EventA>>());

            Mocker.GetMock<IServiceFactory>()
                  .Setup(c => c.BuildAll<IHandleAsync<EventA>>())
                  .Returns(handlers);

            var counter = new ConcurrencyCounter(handlers.Count);


            AsyncHandlerA1.Setup(c => c.HandleAsync(It.IsAny<EventA>()))
                .Callback<EventA>(c =>
                {
                    var id = counter.Start();
                    Thread.Sleep(1000);
                    counter.Stop(id);
                });

            Subject.PublishEvent(eventA);

            counter.WaitForAllItems();

            counter.MaxThreads.Should().Be(3);
        }*/
    }

    public class EventA : IEvent
    {
    }

    public class EventB : IEvent
    {
    }
}
