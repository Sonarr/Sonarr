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
        private CommandQueue _commandQueue;
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

        [TearDown]
        public void TearDown()
        {
            Subject.Handle(new ApplicationShutdownRequested());

            // Give the threads a bit of time to shut down.
            Thread.Sleep(10);
        }

        private void GivenCommandQueue()
        {
            _commandQueue = new CommandQueue();

            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Queue(It.IsAny<CancellationToken>()))
                  .Returns(_commandQueue.GetConsumingEnumerable);
        }

        private void QueueAndWaitForExecution(CommandModel commandModel, bool waitPublish = false)
        {
            var waitEventComplete = new ManualResetEventSlim();
            var waitEventPublish = new ManualResetEventSlim();

            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Complete(It.Is<CommandModel>(c => c == commandModel), It.IsAny<string>()))
                  .Callback(() => waitEventComplete.Set());

            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Fail(It.Is<CommandModel>(c => c == commandModel), It.IsAny<string>(), It.IsAny<Exception>()))
                  .Callback(() => waitEventComplete.Set());

            Mocker.GetMock<IEventAggregator>()
                  .Setup(s => s.PublishEvent<CommandExecutedEvent>(It.IsAny<CommandExecutedEvent>()))
                  .Callback(() => waitEventPublish.Set());

            _commandQueue.Add(commandModel);

            if (!waitEventComplete.Wait(15000))
            {
                Assert.Fail("Command did not Complete/Fail within 15 sec");
            }

            if (waitPublish && !waitEventPublish.Wait(500))
            {
                Assert.Fail("Command did not Publish within 500 msec");
            }
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

            QueueAndWaitForExecution(commandModel);

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

            QueueAndWaitForExecution(commandModel);

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

            QueueAndWaitForExecution(commandModel, true);

            VerifyEventPublished<CommandExecutedEvent>();

            ExceptionVerification.WaitForErrors(1, 500);
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

            QueueAndWaitForExecution(commandModel, true);

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

            QueueAndWaitForExecution(commandModel);

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(s => s.Complete(It.Is<CommandModel>(c => c == commandModel), commandA.CompletionMessage), Times.Once());
        }

        [Test]
        public void should_use_last_progress_message_if_completion_message_is_null()
        {
            GivenCommandQueue();
            var commandB = new CommandB();
            var commandModel = new CommandModel
            {
                Body = commandB,
                Message = "Do work"
            };

            Subject.Handle(new ApplicationStartedEvent());

            QueueAndWaitForExecution(commandModel);

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(s => s.Complete(It.Is<CommandModel>(c => c == commandModel), commandModel.Message), Times.Once());
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
