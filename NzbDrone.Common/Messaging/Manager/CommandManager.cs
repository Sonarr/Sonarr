using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Messaging.Events;

namespace NzbDrone.Common.Messaging.Manager
{
    public interface IManageCommands
    {
        ICollection<CommandManagerItem> Items { get; }
        Boolean ExistingItem(ICommand command);
    }

    public class CommandManager : IManageCommands,
                                       IHandle<CommandStartedEvent>,
                                       IHandle<CommandCompletedEvent>,
                                       IHandle<CommandFailedEvent>
    {
        private readonly ICached<CommandManagerItem> _cache;

        public CommandManager(ICacheManger cacheManger)
        {
            _cache = cacheManger.GetCache<CommandManagerItem>(GetType());
        }

        public void Handle(CommandStartedEvent message)
        {
            _cache.Set(message.Command.CommandId, new CommandManagerItem(message.Command, CommandState.Running));
        }

        public void Handle(CommandCompletedEvent message)
        {
            _cache.Set(message.Command.CommandId, new CommandManagerItem(message.Command, CommandState.Completed));
        }

        public void Handle(CommandFailedEvent message)
        {
            _cache.Set(message.Command.CommandId, new CommandManagerItem(message.Command, CommandState.Failed));
        }

        public ICollection<CommandManagerItem> Items
        {
            get
            {
                return _cache.Values;
            }
        }

        public bool ExistingItem(ICommand command)
        {
            var running = Items.Where(i => i.Type == command.GetType().FullName && i.State == CommandState.Running);

            var result = running.Select(r => r.Command).Contains(command, new CommandEqualityComparer());

            return result;
        }
    }
}
