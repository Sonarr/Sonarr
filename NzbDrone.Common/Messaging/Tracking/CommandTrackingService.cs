using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using NzbDrone.Common.Cache;

namespace NzbDrone.Common.Messaging.Tracking
{
    public interface ITrackCommands
    {
        TrackedCommand TrackIfNew(ICommand command);
        ExistingCommand TrackNewOrGet(ICommand command);
        TrackedCommand Completed(TrackedCommand trackedCommand, TimeSpan runtime);
        TrackedCommand Failed(TrackedCommand trackedCommand, Exception e);
        List<TrackedCommand> AllTracked();
        Boolean ExistingCommand(ICommand command);
        TrackedCommand FindExisting(ICommand command);
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

            var trackedCommand = new TrackedCommand(command, ProcessState.Running);
            Store(trackedCommand);

            return trackedCommand;
        }

        public ExistingCommand TrackNewOrGet(ICommand command)
        {
            var trackedCommand = FindExisting(command);

            if (trackedCommand == null)
            {
                trackedCommand = new TrackedCommand(command, ProcessState.Running);
                Store(trackedCommand);

                return new ExistingCommand(false, trackedCommand);
            }

            return new ExistingCommand(true, trackedCommand);
        }

        public TrackedCommand Completed(TrackedCommand trackedCommand, TimeSpan runtime)
        {
            trackedCommand.StateChangeTime = DateTime.UtcNow;
            trackedCommand.State = ProcessState.Completed;
            trackedCommand.Runtime = runtime;

            Store(trackedCommand);

            return trackedCommand;
        }

        public TrackedCommand Failed(TrackedCommand trackedCommand, Exception e)
        {
            trackedCommand.StateChangeTime = DateTime.UtcNow;
            trackedCommand.State = ProcessState.Failed;
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
            return FindExisting(command) != null;
        }

        public TrackedCommand FindExisting(ICommand command)
        {
            var comparer = new CommandEqualityComparer();
            return Running(command.GetType()).SingleOrDefault(t => comparer.Equals(t.Command, command));
        }

        private List<TrackedCommand> Running(Type type = null)
        {
            var running = AllTracked().Where(i => i.State == ProcessState.Running);

            if (type != null)
            {
                return running.Where(t => t.Type == type.FullName).ToList();
            }

            return running.ToList();
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
            var old = AllTracked().Where(c => c.State != ProcessState.Running && c.StateChangeTime < DateTime.UtcNow.AddMinutes(-5));

            foreach (var trackedCommand in old)
            {
                _cache.Remove(trackedCommand.Command.CommandId);
            }
        }
    }
}
