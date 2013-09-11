using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Messaging
{
    public interface IExecute<TCommand> : IProcessMessage<TCommand> where TCommand : Command
    {
        void Execute(TCommand message);
    }
}