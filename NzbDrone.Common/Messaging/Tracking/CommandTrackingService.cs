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
        List<TrackedCommand> AllTracked();
        Boolean ExistingCommand(ICommand command);
    }

    public class TrackCommands : ITrackCommands, IExecute<TrackedCommandCleanupCommand>
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
            Store(trackedCommand);

            return trackedCommand;
        }

        public TrackedCommand Completed(TrackedCommand trackedCommand, TimeSpan runtime)
        {
            trackedCommand.StateChangeTime = DateTime.UtcNow;
            trackedCommand.State = CommandState.Completed;
            trackedCommand.Runtime = runtime;

            Store(trackedCommand);

            return trackedCommand;
        }

        public TrackedCommand Failed(TrackedCommand trackedCommand, Exception e)
        {
            trackedCommand.StateChangeTime = DateTime.UtcNow;
            trackedCommand.State = CommandState.Failed;
            trackedCommand.Exception = e;

            Store(trackedCommand);

            return trackedCommand;
        }

        public List<TrackedCommand> AllTracked()
        {
            return _cache.Values.ToList();
        }

        public bool ExistingCommand(ICommand command)
        {
            var running = AllTracked().Where(i => i.Type == command.GetType().FullName && i.State == CommandState.Running);

            var result = running.Select(r => r.Command).Contains(command, new CommandEqualityComparer());

            return result;
        }

        private void Store(TrackedCommand trackedCommand)
        {
            if (trackedCommand.Command.GetType() == typeof(TrackedCommandCleanupCommand))
            {
                return;
            }

            _cache.Set(trackedCommand.Command.CommandId, trackedCommand);
        }

        public void Execute(TrackedCommandCleanupCommand message)
        {
            var old = AllTracked().Where(c => c.StateChangeTime < DateTime.UtcNow.AddMinutes(-15));

            foreach (var trackedCommand in old)
            {
                _cache.Remove(trackedCommand.Command.CommandId);
            }
        }
    }
}
