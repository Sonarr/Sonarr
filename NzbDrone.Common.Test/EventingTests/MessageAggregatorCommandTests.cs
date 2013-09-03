using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Messaging.Tracking;
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

            Mocker.GetMock<ITrackCommands>()
                  .Setup(c => c.TrackIfNew(It.IsAny<CommandA>()))
                  .Returns(new TrackedCommand(new CommandA(), ProcessState.Running));

            Mocker.GetMock<ITrackCommands>()
                  .Setup(c => c.TrackIfNew(It.IsAny<CommandB>()))
                  .Returns(new TrackedCommand(new CommandB(), ProcessState.Running));
        }

        [Test]
        public void should_publish_command_to_executor()
        {
            var commandA = new CommandA();

            Mocker.GetMock<ITrackCommands>()
                  .Setup(c => c.TrackIfNew(commandA))
                  .Returns(new TrackedCommand(commandA, ProcessState.Running));

            Subject.PublishCommand(commandA);

            _executorA.Verify(c => c.Execute(commandA), Times.Once());
        }

        [Test]
        public void should_publish_command_by_with_optional_arg_using_name()
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

            Mocker.GetMock<ITrackCommands>()
                  .Setup(c => c.TrackIfNew(commandA))
                  .Returns(new TrackedCommand(commandA, ProcessState.Running));

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
        public String CommandId { get; private set; }
// ReSharper disable UnusedParameter.Local
        public CommandA(int id = 0)
// ReSharper restore UnusedParameter.Local
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }

    public class CommandB : ICommand
    {
        public String CommandId { get; private set; }

        public CommandB()
        {
            CommandId = HashUtil.GenerateCommandId();
        }
    }

}