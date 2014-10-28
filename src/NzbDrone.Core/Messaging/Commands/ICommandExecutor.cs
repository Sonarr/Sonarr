using System;

namespace NzbDrone.Core.Messaging.Commands
{
    public interface ICommandExecutor
    {
        void PublishCommand<TCommand>(TCommand command) where TCommand : Command;
        void PublishCommand(string commandTypeName, DateTime? lastEecutionTime);
        Command PublishCommandAsync<TCommand>(TCommand command) where TCommand : Command;
        Command PublishCommandAsync(string commandTypeName);
    }
}