using System;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EventingTests
{
    [TestFixture]
    public class MessageAggregatorCommandTests : TestBase<MessageAggregator>
    {
        private Mock<IExecute<CommandA>> _executorA;
        private Mock<IExecute<CommandB>> _executorB;

        [SetUp]
        public void Setup()
        {
            _executorA = new Mock<IExecute<CommandA>>();
            _executorB = new Mock<IExecute<CommandB>>();

            Mocker.GetMock<IServiceFactory>()
                  .Setup(c => c.Build(typeof(IExecute<CommandA>)))
                  .Returns(_executorA.Object);

            Mocker.GetMock<IServiceFactory>()
                  .Setup(c => c.Build(typeof(IExecute<CommandB>)))
                  .Returns(_executorB.Object);

        }

        [Test]
        public void should_publish_command_to_executor()
        {
            var commandA = new CommandA();

            Subject.PublishCommand(commandA);

            _executorA.Verify(c => c.Execute(commandA), Times.Once());
        }

        [Test]
        public void should_not_publish_to_incompatible_executor()
        {
            var commandA = new CommandA();


            Subject.PublishCommand(commandA);

            _executorA.Verify(c => c.Execute(commandA), Times.Once());
            _executorB.Verify(c => c.Execute(It.IsAny<CommandB>()), Times.Never());
        }

        [Test]
        public void broken_executor_should_throw_the_exception()
        {
            var commandA = new CommandA();

            _executorA.Setup(c => c.Execute(It.IsAny<CommandA>()))
                       .Throws(new NotImplementedException());

            Assert.Throws<NotImplementedException>(() => Subject.PublishCommand(commandA));
        }
    }

    public class CommandA : ICommand
    {

    }

    public class CommandB : ICommand
    {

    }

}