namespace Workarr.Messaging.Commands
{
    public interface IExecute<TCommand> : IProcessMessage<TCommand>
        where TCommand : Command
    {
        void Execute(TCommand message);
    }
}
