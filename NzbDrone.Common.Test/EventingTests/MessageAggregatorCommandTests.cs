using System;
using System.Collections.Generic;
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
        public void should_publish_command_by_with_optional_arg_with_name()
        {
            Mocker.GetMock<IServiceFactory>().Setup(c => c.GetImplementations(typeof(ICommand)))
                  .Returns(new List<Type> { typeof(CommandA), typeof(CommandB) });

            Subject.PublishCommand(typeof(CommandA).FullName);
            _executorA.Verify(c => c.Execute(It.IsAny<CommandA>()), Times.Once());
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
        public CommandA(int id = 0)
        {

        }
    }

    public class CommandB : ICommand
    {

    }

}