namespace NzbDrone.Common.Messaging
{
    public interface IExecute<TCommand> : IProcessMessage<TCommand> where TCommand : ICommand
    {
        void Execute(TCommand message);
    }
}