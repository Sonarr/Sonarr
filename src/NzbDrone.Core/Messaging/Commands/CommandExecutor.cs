using System;
using System.Threading;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ProgressMessaging;

namespace NzbDrone.Core.Messaging.Commands
{
    public class CommandExecutor : //IHandle<CommandQueuedEvent>,
                                   IHandle<ApplicationStartedEvent>,
                                   IHandle<ApplicationShutdownRequested>
    {
        private readonly Logger _logger;
        private readonly IServiceFactory _serviceFactory;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEventAggregator _eventAggregator;

        private static CancellationTokenSource _cancellationTokenSource;
        private const int THREAD_LIMIT = 3;

        public CommandExecutor(IServiceFactory serviceFactory,
                               IManageCommandQueue commandQueueManager,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
            _commandQueueManager = commandQueueManager;
            _eventAggregator = eventAggregator;
        }

        private void ExecuteCommands()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var command = _commandQueueManager.Pop();

                    if (command != null)
                    {
                        try
                        {
                            ExecuteCommand((dynamic)command.Body, command);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Error occurred while executing task " + command.Name, ex);
                        }
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                Thread.ResetAbort();
            }
        }

        private void ExecuteCommand<TCommand>(TCommand command, CommandModel commandModel) where TCommand : Command
        {
            var handlerContract = typeof(IExecute<>).MakeGenericType(command.GetType());
            var handler = (IExecute<TCommand>)_serviceFactory.Build(handlerContract);

            _logger.Trace("{0} -> {1}", command.GetType().Name, handler.GetType().Name);

            try
            {
                BroadcastCommandUpdate(commandModel);

                if (!MappedDiagnosticsContext.Contains("CommandId") && command.SendUpdatesToClient)
                {
                    MappedDiagnosticsContext.Set("CommandId", commandModel.Id.ToString());
                }

                handler.Execute(command);
                _commandQueueManager.Completed(commandModel);
            }
            catch (Exception e)
            {
                _commandQueueManager.Failed(commandModel, e);
                throw;
            }
            finally
            {
                BroadcastCommandUpdate(commandModel);

                _eventAggregator.PublishEvent(new CommandExecutedEvent(commandModel));

                if (MappedDiagnosticsContext.Get("CommandId") == commandModel.Id.ToString())
                {
                    MappedDiagnosticsContext.Remove("CommandId");
                }
            }

            _logger.Trace("{0} <- {1} [{2}]", command.GetType().Name, handler.GetType().Name, commandModel.Duration.ToString());
        }
        
        private void BroadcastCommandUpdate(CommandModel command)
        {
            if (command.Body.SendUpdatesToClient)
            {
                _eventAggregator.PublishEvent(new CommandUpdatedEvent(command));
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            for (int i = 0; i < THREAD_LIMIT; i++)
            {
                var thread = new Thread(ExecuteCommands);
                thread.Start();
            }
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            _logger.Info("Shutting down task execution");
            _cancellationTokenSource.Cancel(true);
        }
    }
}
