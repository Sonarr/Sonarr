namespace NzbDrone.Core.Messaging.Commands
{
    public interface ICommandExecutor
    {
        void PublishCommand<TCommand>(TCommand command) where TCommand : Command;
        void PublishCommand(string commandTypeName);
        Command PublishCommandAsync<TCommand>(TCommand command) where TCommand : Command;
        Command PublishCommandAsync(string commandTypeName);
    }
}