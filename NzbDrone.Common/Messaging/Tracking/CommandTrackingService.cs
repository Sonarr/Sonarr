using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;

namespace NzbDrone.Common.Messaging.Tracking
{
    public interface ITrackCommands
    {
        TrackedCommand TrackIfNew(ICommand command);
        TrackedCommand Completed(TrackedCommand trackedCommand, TimeSpan runtime);
        TrackedCommand Failed(TrackedCommand trackedCommand, Exception e);
        ICollection<TrackedCommand> AllTracked { get; }
        Boolean ExistingCommand(ICommand command);
    }

    public class TrackCommands : ITrackCommands
    {
        private readonly ICached<TrackedCommand> _cache;

        public TrackCommands(ICacheManger cacheManger)
        {
            _cache = cacheManger.GetCache<TrackedCommand>(GetType());
        }

        public TrackedCommand TrackIfNew(ICommand command)
        {
            if (ExistingCommand(command))
            {
                return null;
            }

            var trackedCommand = new TrackedCommand(command, CommandState.Running);
            _cache.Set(command.CommandId, trackedCommand);

            return trackedCommand;
        }

        public TrackedCommand Completed(TrackedCommand trackedCommand, TimeSpan runtime)
        {
            trackedCommand.StateChangeTime = DateTime.UtcNow;
            trackedCommand.State = CommandState.Completed;
            trackedCommand.Runtime = runtime;

            _cache.Set(trackedCommand.Command.CommandId, trackedCommand);

            return trackedCommand;
        }

        public TrackedCommand Failed(TrackedCommand trackedCommand, Exception e)
        {
            trackedCommand.StateChangeTime = DateTime.UtcNow;
            trackedCommand.State = CommandState.Failed;
            trackedCommand.Exception = e;

            _cache.Set(trackedCommand.Command.CommandId, trackedCommand);

            return trackedCommand;
        }

        public ICollection<TrackedCommand> AllTracked
        {
            get
            {
                return _cache.Values;
            }
        }

        public bool ExistingCommand(ICommand command)
        {
            var running = AllTracked.Where(i => i.Type == command.GetType().FullName && i.State == CommandState.Running);

            var result = running.Select(r => r.Command).Contains(command, new CommandEqualityComparer());

            return result;
        }
    }
}
