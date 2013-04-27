using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EventingTests
{
    [TestFixture]
    public class MessageAggregatorCommandTests : TestBase
    {
        [Test]
        public void should_publish_command_to_executor()
        {
            var commandA = new CommandA();

            var executor = new Mock<IExecute<CommandA>>();
            var aggregator = new MessageAggregator(TestLogger, () => new List<IProcessMessage> { executor.Object });
            aggregator.PublishCommand(commandA);

            executor.Verify(c => c.Execute(commandA), Times.Once());
        }


        [Test]
        public void should_throw_if_more_than_one_handler()
        {
            var commandA = new CommandA();

            var executor1 = new Mock<IExecute<CommandA>>();
            var executor2 = new Mock<IExecute<CommandA>>();
            var aggregator = new MessageAggregator(TestLogger, () => new List<IProcessMessage> { executor1.Object, executor2.Object });

            Assert.Throws<InvalidOperationException>(() => aggregator.PublishCommand(commandA));
        }

        [Test]
        public void should_not_publish_to_incompatible_executor()
        {
            var commandA = new CommandA();

            var executor1 = new Mock<IExecute<CommandA>>();
            var executor2 = new Mock<IExecute<CommandB>>();
            var aggregator = new MessageAggregator(TestLogger, () => new List<IProcessMessage> { executor1.Object, executor2.Object });

            aggregator.PublishCommand(commandA);

            executor1.Verify(c => c.Execute(commandA), Times.Once());
            executor2.Verify(c => c.Execute(It.IsAny<CommandB>()), Times.Never());
        }

        [Test]
        public void broken_executor_should_throw_the_exception()
        {
            var commandA = new CommandA();

            var executor = new Mock<IExecute<CommandA>>();

            executor.Setup(c => c.Execute(It.IsAny<CommandA>()))
                       .Throws(new NotImplementedException());

            var aggregator = new MessageAggregator(TestLogger, () => new List<IProcessMessage> { executor.Object });
            Assert.Throws<NotImplementedException>(() => aggregator.PublishCommand(commandA));
        }
    }

    public class CommandA : ICommand
    {

    }

    public class CommandB : ICommand
    {

    }

}