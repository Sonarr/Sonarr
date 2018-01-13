using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Messaging.Commands
{
    public interface IManageCommandQueue
    {
        List<CommandModel> PushMany<TCommand>(List<TCommand> commands) where TCommand : Command;
        CommandModel Push<TCommand>(TCommand command, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified) where TCommand : Command;
        CommandModel Push(string commandName, DateTime? lastExecutionTime, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified);
        IEnumerable<CommandModel> Queue(CancellationToken cancellationToken);
        CommandModel Get(int id);
        List<CommandModel> GetStarted(); 
        void SetMessage(CommandModel command, string message);
        void Start(CommandModel command);
        void Complete(CommandModel command, string message);
        void Fail(CommandModel command, string message, Exception e);
        void Requeue();
        void CleanCommands();
    }

    public class CommandQueueManager : IManageCommandQueue, IHandle<ApplicationStartedEvent>
    {
        private readonly ICommandRepository _repo;
        private readonly IServiceFactory _serviceFactory;
        private readonly Logger _logger;

        private readonly ICached<CommandModel> _commandCache;
        private readonly BlockingCollection<CommandModel> _commandQueue;

        public CommandQueueManager(ICommandRepository repo, 
                                   IServiceFactory serviceFactory,
                                   ICacheManager cacheManager,
                                   Logger logger)
        {
            _repo = repo;
            _serviceFactory = serviceFactory;
            _logger = logger;

            _commandCache = cacheManager.GetCache<CommandModel>(GetType());
            _commandQueue = new BlockingCollection<CommandModel>(new CommandQueue());
        }

        public List<CommandModel> PushMany<TCommand>(List<TCommand> commands) where TCommand : Command
        {
            _logger.Trace("Publishing {0} commands", commands.Count);

            var commandModels = new List<CommandModel>();
            var existingCommands = _commandCache.Values.Where(q => q.Status == CommandStatus.Queued ||
                                                              q.Status == CommandStatus.Started).ToList();

            foreach (var command in commands)
            {
                var existing = existingCommands.SingleOrDefault(c => c.Name == command.Name && CommandEqualityComparer.Instance.Equals(c.Body, command));

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
                _commandCache.Set(commandModel.Id.ToString(), commandModel);
                _commandQueue.Add(commandModel);
            }

            return commandModels;
        }

        public CommandModel Push<TCommand>(TCommand command, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified) where TCommand : Command
        {
            Ensure.That(command, () => command).IsNotNull();

            _logger.Trace("Publishing {0}", command.Name);
            _logger.Trace("Checking if command is queued or started: {0}", command.Name);

            var existingCommands = QueuedOrStarted(command.Name);
            var existing = existingCommands.SingleOrDefault(c => CommandEqualityComparer.Instance.Equals(c.Body, command));

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
            _commandCache.Set(commandModel.Id.ToString(), commandModel);
            _commandQueue.Add(commandModel);

            return commandModel;
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

        public CommandModel Get(int id)
        {
            return _commandCache.Get(id.ToString(), () => FindCommand(_repo.Get(id)));
        }

        public List<CommandModel> GetStarted()
        {
            _logger.Trace("Getting started commands");
            return _commandCache.Values.Where(c => c.Status == CommandStatus.Started).ToList();
        }

        public void SetMessage(CommandModel command, string message)
        {
            command.Message = message;
            _commandCache.Set(command.Id.ToString(), command);
        }

        public void Start(CommandModel command)
        {
            command.StartedAt = DateTime.UtcNow;
            command.Status = CommandStatus.Started;

            _logger.Trace("Marking command as started: {0}", command.Name);
            _commandCache.Set(command.Id.ToString(), command);         
            _repo.Start(command);
        }

        public void Complete(CommandModel command, string message)
        {
            Update(command, CommandStatus.Completed, message);
        }

        public void Fail(CommandModel command, string message, Exception e)
        {
            command.Exception = e.ToString();
            
            Update(command, CommandStatus.Failed, message);
        }

        public void Requeue()
        {
            foreach (var command in _repo.Queued())
            {
                _commandQueue.Add(command);
            }
        }

        public void CleanCommands()
        {
            _logger.Trace("Cleaning up old commands");
            
            var old = _commandCache.Values.Where(c => c.EndedAt < DateTime.UtcNow.AddMinutes(-5));

            foreach (var command in old)
            {
                _commandCache.Remove(command.Id.ToString());
            }

            _repo.Trim();
        }

        private dynamic GetCommand(string commandName)
        {
            commandName = commandName.Split('.').Last();

            var commandType = _serviceFactory.GetImplementations(typeof(Command))
                                             .Single(c => c.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));

            return Json.Deserialize("{}", commandType);
        }

        private CommandModel FindCommand(CommandModel command)
        {
            var cachedCommand = _commandCache.Find(command.Id.ToString());

            if (cachedCommand != null)
            {
                command.Message = cachedCommand.Message;
            }

            return command;
        }

        private void Update(CommandModel command, CommandStatus status, string message)
        {
            SetMessage(command, message);

            command.EndedAt = DateTime.UtcNow;
            command.Duration = command.EndedAt.Value.Subtract(command.StartedAt.Value);
            command.Status = status;

            _logger.Trace("Updating command status");
            _commandCache.Set(command.Id.ToString(), command);
            _repo.End(command);
        }

        private List<CommandModel> QueuedOrStarted(string name)
        {
            return _commandCache.Values.Where(q => q.Name == name &&
                                                   (q.Status == CommandStatus.Queued ||
                                                    q.Status == CommandStatus.Started)).ToList();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Trace("Orphaning incomplete commands");
            _repo.OrphanStarted();
            Requeue();
        }
    }
}
