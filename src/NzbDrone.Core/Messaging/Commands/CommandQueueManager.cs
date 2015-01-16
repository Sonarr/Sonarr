using System;
using System.Collections.Generic;
using System.Linq;
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
        CommandModel Push<TCommand>(TCommand command, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified) where TCommand : Command;
        CommandModel Push(string commandName, DateTime? lastExecutionTime, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified);
        CommandModel Pop();
        CommandModel Get(int id);
        List<CommandModel> GetStarted(); 
        void SetMessage(CommandModel command, string message);
        void Completed(CommandModel command);
        void Failed(CommandModel command, Exception e);
    }

    public class CommandQueueManager : IManageCommandQueue, IHandle<ApplicationStartedEvent>
    {
        private readonly ICommandRepository _repo;
        private readonly IServiceFactory _serviceFactory;
        private readonly Logger _logger;

        private ICached<string> _messageCache; 

        private static readonly object Mutex = new object();

        public CommandQueueManager(ICommandRepository repo, 
                              IServiceFactory serviceFactory,
                              ICacheManager cacheManager,
                              Logger logger)
        {
            _repo = repo;
            _serviceFactory = serviceFactory;
            _logger = logger;

            _messageCache = cacheManager.GetCache<string>(GetType());
        }

        public CommandModel Push<TCommand>(TCommand command, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified) where TCommand : Command
        {
            Ensure.That(command, () => command).IsNotNull();

            _logger.Trace("Publishing {0}", command.GetType().Name);

            lock (Mutex)
            {
                var existingCommands = _repo.FindQueuedOrStarted(command.Name);
                var existing = existingCommands.SingleOrDefault(c => CommandEqualityComparer.Instance.Equals(c.Body, command));

                if (existing != null)
                {
                    _logger.Trace("Command is already in progress: {0}", command.GetType().Name);

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

                _repo.Insert(commandModel);

                return commandModel;
            }
        }

        public CommandModel Push(string commandName, DateTime? lastExecutionTime, CommandPriority priority = CommandPriority.Normal, CommandTrigger trigger = CommandTrigger.Unspecified)
        {
            dynamic command = GetCommand(commandName);
            command.LastExecutionTime = lastExecutionTime;
            command.Trigger = trigger;

            return Push(command, priority);
        }

        public CommandModel Pop()
        {
            lock (Mutex)
            {
                var nextCommand = _repo.Next();

                if (nextCommand == null)
                {
                    return null;
                }

                nextCommand.StartedAt = DateTime.UtcNow;
                nextCommand.Status = CommandStatus.Started;

                _repo.Update(nextCommand);

                return nextCommand;
            }
        }

        public CommandModel Get(int id)
        {
            return FindMessage(_repo.Get(id));
        }

        public List<CommandModel> GetStarted()
        {
            return _repo.Started();
        }

        public void SetMessage(CommandModel command, string message)
        {
            _messageCache.Set(command.Id.ToString(), message);
        }

        public void Completed(CommandModel command)
        {
            command.EndedAt = DateTime.UtcNow;
            command.Duration = command.EndedAt.Value.Subtract(command.StartedAt.Value);
            command.Status = CommandStatus.Completed;

            _repo.Update(command);

            _messageCache.Remove(command.Id.ToString());
        }

        public void Failed(CommandModel command, Exception e)
        {
            command.EndedAt = DateTime.UtcNow;
            command.Duration = command.EndedAt.Value.Subtract(command.StartedAt.Value);
            command.Status = CommandStatus.Failed;

            _repo.Update(command);

            _messageCache.Remove(command.Id.ToString());
        }

        private dynamic GetCommand(string commandName)
        {
            commandName = commandName.Split('.').Last();

            var commandType = _serviceFactory.GetImplementations(typeof(Command))
                                             .Single(c => c.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));

            return Json.Deserialize("{}", commandType);
        }

        private CommandModel FindMessage(CommandModel command)
        {
            command.Message = _messageCache.Find(command.Id.ToString());

            return command;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _repo.OrphanStarted();
        }
    }
}
