using System;
using FluentMigrator.Runner;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Commands.Tracking;

namespace NzbDrone.Core.Messaging.Commands
{
    public abstract class Command : ModelBase, IMessage
    {
        private static readonly object Mutex = new object();
        private static int _idCounter;
        private readonly StopWatch _stopWatch;

        public CommandStatus State { get; private set; }
        public DateTime StateChangeTime { get; private set; }

        public virtual bool SendUpdatesToClient
        {
            get
            {
                return false;
            }
        }

        public TimeSpan Runtime
        {
            get
            {
                return _stopWatch.ElapsedTime();
            }
        }

        public Exception Exception { get; private set; }
        public string Message { get; private set; }

        public string Name { get; private set; }

        protected Command()
        {
            Name = GetType().Name.Replace("Command", "");
            StateChangeTime = DateTime.UtcNow;
            State = CommandStatus.Pending;
            _stopWatch = new StopWatch();

            lock (Mutex)
            {
                Id = ++_idCounter;
            }
        }

        public void Start()
        {
            _stopWatch.Start();
            StateChangeTime = DateTime.UtcNow;
            State = CommandStatus.Running;
            SetMessage("Starting");
        }

        public void Failed(Exception exception)
        {
            _stopWatch.Stop();
            StateChangeTime = DateTime.UtcNow;
            State = CommandStatus.Failed;
            Exception = exception;
            SetMessage("Failed");
        }

        public void Completed()
        {
            _stopWatch.Stop();
            StateChangeTime = DateTime.UtcNow;
            State = CommandStatus.Completed;
            SetMessage("Completed");
        }

        public void SetMessage(string message)
        {
            Message = message;
        }
    }
}