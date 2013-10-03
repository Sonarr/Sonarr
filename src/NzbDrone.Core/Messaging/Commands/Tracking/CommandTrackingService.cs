using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;

namespace NzbDrone.Core.Messaging.Commands.Tracking
{
    public interface ITrackCommands
    {
        Command GetById(int id);
        Command GetById(string id);
        void Completed(Command trackedCommand);
        void Failed(Command trackedCommand, Exception e);
        IEnumerable<Command> RunningCommands();
        Command FindExisting(Command command);
        void Store(Command command);
        void Start(Command command);
    }

    public class CommandTrackingService : ITrackCommands, IExecute<TrackedCommandCleanupCommand>
    {
        private readonly ICached<Command> _cache;

        public CommandTrackingService(ICacheManger cacheManger)
        {
            _cache = cacheManger.GetCache<Command>(GetType());
        }

        public Command GetById(int id)
        {
            return _cache.Find(id.ToString());
        }

        public Command GetById(string id)
        {
            return _cache.Find(id);
        }

        public void Start(Command command)
        {
            command.Start();
        }

        public void Completed(Command trackedCommand)
        {
            trackedCommand.Completed();
        }

        public void Failed(Command trackedCommand, Exception e)
        {
            trackedCommand.Failed(e);
        }

        public IEnumerable<Command> RunningCommands()
        {
            return _cache.Values.Where(c => c.State == CommandStatus.Running);
        }

        public Command FindExisting(Command command)
        {
            return RunningCommands().SingleOrDefault(t => CommandEqualityComparer.Instance.Equals(t, command));
        }

        public void Store(Command command)
        {
            if (command.GetType() == typeof(TrackedCommandCleanupCommand))
            {
                return;
            }

            _cache.Set(command.Id.ToString(), command);
        }

        public void Execute(TrackedCommandCleanupCommand message)
        {
            var old = _cache.Values.Where(c => c.State != CommandStatus.Running && c.StateChangeTime < DateTime.UtcNow.AddMinutes(-5));

            foreach (var trackedCommand in old)
            {
                _cache.Remove(trackedCommand.Id.ToString());
            }
        }
    }
}
