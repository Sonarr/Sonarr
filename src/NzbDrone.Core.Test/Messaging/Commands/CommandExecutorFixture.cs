using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Messaging.Commands
{
    [TestFixture]
    public class CommandExecutorFixture : TestBase<CommandExecutor>
    {
        private BlockingCollection<CommandModel> _commandQueue;
        private Mock<IExecute<CommandA>> _executorA;
        private Mock<IExecute<CommandB>> _executorB;
        private bool _commandExecuted = false;

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

        private void GivenCommandQueue()
        {
            _commandQueue = new BlockingCollection<CommandModel>(new CommandQueue());

            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Queue(It.IsAny<CancellationToken>()))
                  .Returns(_commandQueue.GetConsumingEnumerable);
        }

        private void WaitForExecution(CommandModel commandModel)
        {
            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Complete(It.Is<CommandModel>(c => c == commandModel), It.IsAny<string>()))
                  .Callback(() => _commandExecuted = true);

            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Fail(It.Is<CommandModel>(c => c == commandModel), It.IsAny<string>(), It.IsAny<Exception>()))
                  .Callback(() => _commandExecuted = true);

            while (!_commandExecuted)
            {
                Thread.Sleep(100);
            }

            var t1 = 1;
        }

        [Test]
        public void should_start_executor_threads()
        {
            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Queue(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }

        [Test]
        public void should_execute_on_executor()
        {
            GivenCommandQueue();
            var commandA = new CommandA();
            var commandModel = new CommandModel
                               {
                                   Body = commandA
                               };

            Subject.Handle(new ApplicationStartedEvent());
            _commandQueue.Add(commandModel);

            WaitForExecution(commandModel);

            _executorA.Verify(c => c.Execute(commandA), Times.Once());
        }

        [Test]
        public void should_not_execute_on_incompatible_executor()
        {
            GivenCommandQueue();
            var commandA = new CommandA();
            var commandModel = new CommandModel
            {
                Body = commandA
            };

            Subject.Handle(new ApplicationStartedEvent());
            _commandQueue.Add(commandModel);

            WaitForExecution(commandModel);

            _executorA.Verify(c => c.Execute(commandA), Times.Once());
            _executorB.Verify(c => c.Execute(It.IsAny<CommandB>()), Times.Never());
        }

        [Test]
        public void broken_executor_should_publish_executed_event()
        {
            GivenCommandQueue();
            var commandA = new CommandA();
            var commandModel = new CommandModel
            {
                Body = commandA
            };

            _executorA.Setup(s => s.Execute(It.IsAny<CommandA>()))
                      .Throws(new NotImplementedException());

            Subject.Handle(new ApplicationStartedEvent());
            _commandQueue.Add(commandModel);

            WaitForExecution(commandModel);

            VerifyEventPublished<CommandExecutedEvent>();
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_publish_executed_event_on_success()
        {
            GivenCommandQueue();
            var commandA = new CommandA();
            var commandModel = new CommandModel
            {
                Body = commandA
            };

            Subject.Handle(new ApplicationStartedEvent());
            _commandQueue.Add(commandModel);

            WaitForExecution(commandModel);

            VerifyEventPublished<CommandExecutedEvent>();
        }

        [Test]
        public void should_use_completion_message()
        {
            GivenCommandQueue();
            var commandA = new CommandA();
            var commandModel = new CommandModel
            {
                Body = commandA
            };

            Subject.Handle(new ApplicationStartedEvent());
            _commandQueue.Add(commandModel);

            WaitForExecution(commandModel);

            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Complete(It.Is<CommandModel>(c => c == commandModel), commandA.CompletionMessage))
                  .Callback(() => _commandExecuted = true);
        }

        [Test]
        public void should_use_last_progress_message_if_completion_message_is_null()
        {
            GivenCommandQueue();
            var commandA = new CommandA();
            var commandModel = new CommandModel
            {
                Body = commandA,
                Message = "Do work"
            };

            Subject.Handle(new ApplicationStartedEvent());
            _commandQueue.Add(commandModel);

            WaitForExecution(commandModel);

            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Complete(It.Is<CommandModel>(c => c == commandModel), commandModel.Message))
                  .Callback(() => _commandExecuted = true);
        }
    }

    public class CommandA : Command
    {
        public CommandA(int id = 0)
        {
        }
    }

    public class CommandB : Command
    {

        public CommandB()
        {
            
        }

        public override string CompletionMessage => null;
    }

}