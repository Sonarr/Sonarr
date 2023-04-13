using NzbDrone.Core.ProgressMessaging;

namespace NzbDrone.Core.Messaging.Commands
{
    public interface ICommandResultReporter
    {
        void Report(CommandResult result);
    }

    public class CommandResultReporter : ICommandResultReporter
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public CommandResultReporter(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Report(CommandResult result)
        {
            var command = ProgressMessageContext.CommandModel;

            if (command == null)
            {
                return;
            }

            if (!ProgressMessageContext.LockReentrancy())
            {
                return;
            }

            try
            {
                _commandQueueManager.SetResult(command, result);
            }
            finally
            {
                ProgressMessageContext.UnlockReentrancy();
            }
        }
    }
}
