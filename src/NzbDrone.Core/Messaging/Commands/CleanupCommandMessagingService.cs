namespace NzbDrone.Core.Messaging.Commands
{
    public class CleanupCommandMessagingService : IExecute<MessagingCleanupCommand>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public CleanupCommandMessagingService(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Execute(MessagingCleanupCommand message)
        {
            _commandQueueManager.CleanCommands();
        }
    }
}
