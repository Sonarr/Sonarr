using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Messaging.Commands
{
    public interface IManageCommandQueue
    {
        List<CommandModel> PushMany<TCommand>(List<TCommand> commands)
            where TCommand : Command;
        CommandModel Push<TCommand>(TCommand command, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified)
            where TCommand : Command;
        CommandModel Push(string commandName, DateTime? lastExecutionTime, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified);
        IEnumerable<CommandModel> Queue(CancellationToken cancellationToken);
        List<CommandModel> All();
        CommandModel Get(int id);
        List<CommandModel> GetStarted();
        void SetMessage(CommandModel command, string message);
        void Start(CommandModel command);
        void Complete(CommandModel command, string message);
        void Fail(CommandModel command, string message, Exception e);
        void Requeue();
        void Cancel(int id);
        void CleanCommands();
    }

    public class CommandQueueManager : IManageCommandQueue, IHandle<ApplicationStartedEvent>
    {
        private readonly ICommandRepository _repo;
        private readonly KnownTypes _knownTypes;
        private readonly Logger _logger;

        private readonly CommandQueue _commandQueue;

        public CommandQueueManager(ICommandRepository repo,
                                   IServiceFactory serviceFactory,
                                   KnownTypes knownTypes,
                                   Logger logger)
        {
            _repo = repo;
            _knownTypes = knownTypes;
            _logger = logger;

            _commandQueue = new CommandQueue();
        }

        public List<CommandModel> PushMany<TCommand>(List<TCommand> commands)
            where TCommand : Command
        {
            _logger.Trace("Publishing {0} commands", commands.Count);

            lock (_commandQueue)
            {
                var commandModels = new List<CommandModel>();
                var existingCommands = _commandQueue.QueuedOrStarted();

                foreach (var command in commands)
                {
                    var existing = existingCommands.FirstOrDefault(c => c.Name == command.Name && CommandEqualityComparer.Instance.Equals(c.Body, command));

                    if (existing != null)
                    {
                        continue;
                    }

                    var commandModel = new CommandModel
                    {
                        Name = command.Name,
                        Body = command,
                        QueuedAt = DateTime.UtcNow,
                        Trigger = CommandTrigger.Unspecified,
                        Priority = CommandPriority.Normal,
                        Status = CommandStatus.Queued
                    };

                    commandModels.Add(commandModel);
                }

                _repo.InsertMany(commandModels);

                foreach (var commandModel in commandModels)
                {
                    _commandQueue.Add(commandModel);
                }

                return commandModels;
            }
        }

        public CommandModel Push<TCommand>(TCommand command, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified)
            where TCommand : Command
        {
            Ensure.That(command, () => command).IsNotNull();

            _logger.Trace("Publishing {0}", command.Name);
            _logger.Trace("Checking if command is queued or started: {0}", command.Name);

            lock (_commandQueue)
            {
                var existingCommands = QueuedOrStarted(command.Name);
                var existing = existingCommands.FirstOrDefault(c => CommandEqualityComparer.Instance.Equals(c.Body, command));

                if (existing != null)
                {
                    _logger.Trace("Command is already in progress: {0}", command.Name);

                    return existing;
                }

                var commandModel = new CommandModel
                {
                    Name = command.Name,
                    Body = command,
                    QueuedAt = DateTime.UtcNow,
                    Trigger = trigger,
                    Priority = priority,
                    Status = CommandStatus.Queued
                };

                _logger.Trace("Inserting new command: {0}", commandModel.Name);

                _repo.Insert(commandModel);
                _commandQueue.Add(commandModel);

                return commandModel;
            }
        }

        public CommandModel Push(string commandName, DateTime? lastExecutionTime, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified)
        {
            dynamic command = GetCommand(commandName);
            command.LastExecutionTime = lastExecutionTime;
            command.Trigger = trigger;

            return Push(command, priority, trigger);
        }

        public IEnumerable<CommandModel> Queue(CancellationToken cancellationToken)
        {
            return _commandQueue.GetConsumingEnumerable(cancellationToken);
        }

        public List<CommandModel> All()
        {
            _logger.Trace("Getting all commands");
            return _commandQueue.All();
        }

        public CommandModel Get(int id)
        {
            var command = _commandQueue.Find(id);

            if (command == null)
            {
                command = _repo.Get(id);
            }

            return command;
        }

        public List<CommandModel> GetStarted()
        {
            _logger.Trace("Getting started commands");
            return _commandQueue.All().Where(c => c.Status == CommandStatus.Started).ToList();
        }

        public void SetMessage(CommandModel command, string message)
        {
            command.Message = message;
        }

        public void Start(CommandModel command)
        {
            // Marks the command as started in the DB, the queue takes care of marking it as started on it's own
            _logger.Trace("Marking command as started: {0}", command.Name);
            _repo.Start(command);
        }

        public void Complete(CommandModel command, string message)
        {
            Update(command, CommandStatus.Completed, message);

            _commandQueue.PulseAllConsumers();
        }

        public void Fail(CommandModel command, string message, Exception e)
        {
            command.Exception = e.ToString();

            Update(command, CommandStatus.Failed, message);

            _commandQueue.PulseAllConsumers();
        }

        public void Requeue()
        {
            foreach (var command in _repo.Queued())
            {
                _commandQueue.Add(command);
            }
        }

        public void Cancel(int id)
        {
            if (!_commandQueue.RemoveIfQueued(id))
            {
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Unable to cancel task");
            }
        }

        public void CleanCommands()
        {
            _logger.Trace("Cleaning up old commands");

            var commands = _commandQueue.All()
                                        .Where(c => c.EndedAt < DateTime.UtcNow.AddMinutes(-5))
                                        .ToList();

            _commandQueue.RemoveMany(commands);

            _repo.Trim();
        }

        private dynamic GetCommand(string commandName)
        {
            commandName = commandName.Split('.').Last();
            var commands = _knownTypes.GetImplementations(typeof(Command));
            var commandType = commands.Single(c => c.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));

            return Json.Deserialize("{}", commandType);
        }

        private void Update(CommandModel command, CommandStatus status, string message)
        {
            SetMessage(command, message);

            command.EndedAt = DateTime.UtcNow;
            command.Duration = command.EndedAt.Value.Subtract(command.StartedAt.Value);
            command.Status = status;

            _logger.Trace("Updating command status");
            _repo.End(command);
        }

        private List<CommandModel> QueuedOrStarted(string name)
        {
            return _commandQueue.QueuedOrStarted()
                                .Where(q => q.Name == name)
                                .ToList();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Trace("Orphaning incomplete commands");
            _repo.OrphanStarted();
            Requeue();
        }
    }
}
