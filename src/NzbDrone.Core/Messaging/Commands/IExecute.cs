namespace NzbDrone.Core.Messaging.Commands
{
    public interface IExecute<TCommand> : IProcessMessage<TCommand>
        where TCommand : Command
    {
        void Execute(TCommand message);
    }
}
